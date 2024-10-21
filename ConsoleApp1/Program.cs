using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using var db = new DogOwnersClubContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
db.Add(new Person { Name = new FullName("John", "Doe"), Duration = new Duration(1, 2) });
db.SaveChanges();

class DogOwnersClubContext : DbContext
{
    public DbSet<Person> Owners { get; set; }
    public DbSet<Dog> Dogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new DogConfiguration());
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
class Dog
{
    public int Id { get; set; }
    public DateOnly DOB { get; set; }
    public Person Owner { get; set; } = null!;
    public List<ShowResult> ShowResults { get; set; } = [];
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
        builder.ComplexProperty(x => x.Name);
    }
}
class DogConfiguration : IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
        builder.OwnsMany(x => x.ShowResults).ToJson();
    }
}

record Duration(int Minutes, int Seconds);
record FullName(string FirstName, string LastName);
