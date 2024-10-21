using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using var db = new DogOwnersClubContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
var p = new Person { Name = new FullName { FirstName = "John", LastName = "Doe", foo = new Foo { Bar = "Test" } }, Duration = new Duration(1, 2) };
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
db.Add(new BigDog { DOB = new DateOnly(2021, 1, 1), Weight = 10, Owner = p });
db.SaveChanges();
db.Dogs.Where(x => x is SmallDog).Load();


class DogOwnersClubContext : DbContext
{
    public DbSet<Person> Owners { get; set; }
    public DbSet<Dog> Dogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new DogConfiguration());

        //modelBuilder.Entity<BigDog>(b =>
        //{
        //    b.UseTphMappingStrategy();
        //    b.HasDiscriminator<string>("D")
        //        .HasValue<BigDog>("B");
        //});
        //modelBuilder.Entity<SmallDog>(b =>
        //{
        //    b.UseTphMappingStrategy();
        //    b.HasDiscriminator<string>("D")
        //        .HasValue<SmallDog>("S");
        //});
        modelBuilder.Entity<BigDog>(b =>
        {
        });
        modelBuilder.Entity<SmallDog>(b =>
        {
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
    public ICollection<Dog> Dogs { get; set; } = [];
    public Duration Duration { get; set; } = null!;
}
abstract class Dog
{
    public int Id { get; set; }
    public DateOnly DOB { get; set; }
    public Person Owner { get; set; } = null!;
}
class BigDog : Dog
{
    public int Weight { get; set; }
}
class SmallDog : Dog
{
    public float Dummy { get; set; }
}

class ShowResult
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
}

class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);
        builder.ToTable("Owners");
        builder.OwnsOne(x => x.Duration).ToJson();
        builder.ComplexProperty(x => x.Name, x => x.ComplexProperty(y => y.foo));
    }
}
class DogConfiguration : IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}

record Duration(int Minutes, int Seconds);
class FullName
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Foo foo { get; set; }
}
class Foo
{
    public string Bar { get; set; }
}
