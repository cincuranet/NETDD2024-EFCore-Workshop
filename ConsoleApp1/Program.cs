using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using var db = new DogOwnersClubContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
var p = new Person { Name = new FullName { FirstName = "John", LastName = "Doe" } };
db.Add(p);
//db.Add(new Dog()
//{
//    Owner = p,
//    DOB = new DateOnly(2021, 1, 1),
//    ShowResults = [new ShowResult { Rating = 1, Comment = "Good" }],
//    VetVisits = [new DateOnly(2021, 1, 1), new DateOnly(2021, 1, 1)],
//});
//db.SaveChanges();
//db.Owners.Where(x => x.Name.FirstName.StartsWith("A")).Load();
//db.Owners.Where(x => x.Duration.Minutes > 10).Load();
//db.Dogs.Where(x => x.VetVisits.Last() < DateOnly.FromDateTime(DateTime.Now.AddDays(-90))).Load();
//db.Add(new BigDog { DOB = new DateOnly(2021, 1, 1), Weight = 10, Owner = p });
//db.SaveChanges();
//db.Dogs.OrderBy(x => x is BigDog ? (x as BigDog).Weight : x is SmallDog ? (x as SmallDog).Dummy : 0).Load();


class DogOwnersClubContext : DbContext
{
    public DbSet<Person> Owners { get; set; }
    public DbSet<Dog> Dogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new DogConfiguration());

        modelBuilder.Entity<Dog2Person>(b =>
        {
            b.HasKey(x => new { x.DogId, x.PersonId });
            b.HasOne(x => x.Dog)
                .WithMany(x => x.D2P);
            b.HasOne(x => x.Person)
                .WithMany(x => x.D2P);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer("Data Source=2001:67c:d74:66:5cbb:f6ff:fe9e:eefa;Database=dogs;User=sa;Password=Pa$$w0rd;Connect Timeout=10;ConnectRetryCount=0;TrustServerCertificate=true");
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
    }
}

class Person
{
    public int Id { get; set; }
    public FullName Name { get; set; } = null!;
    //public ICollection<Dog> Dogs { get; set; } = [];
    public ICollection<Dog2Person> D2P { get; set; } = [];
}
class Dog
{
    public int Id { get; set; }
    public DateOnly DOB { get; set; }
    //public ICollection<Person> Owners { get; set; } = [];
    public ICollection<Dog2Person> D2P { get; set; } = [];
}
class Dog2Person
{
    public int DogId { get; set; }
    public int PersonId { get; set; }
    public Dog Dog { get; set; } = null!;
    public Person Person { get; set; } = null!;
    public DateTime Created { get; set; }
}

class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ComplexProperty(x => x.Name);
        builder.ToTable("Owners");
    }
}
class DogConfiguration : IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
        //builder.HasMany(x => x.Owners)
        //    .WithMany(x => x.Dogs);
    }
}

class FullName
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
