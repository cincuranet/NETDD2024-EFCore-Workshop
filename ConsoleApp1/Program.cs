using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using var db = new DogOwnersClubContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();


class DogOwnersClubContext : DbContext
{
    public DbSet<Person> Owners { get; set; }
    public DbSet<Dog> Dogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer("Data Source=2001:67c:d74:66:5cbb:f6ff:fe9e:eefa;Database=dogs;User=sa;Password=Pa$$w0rd;Connect Timeout=10;ConnectRetryCount=0;TrustServerCertificate=true");
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
    }
}

//[Table("T_OWNER")]
class Person
{
    //[Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Dog> Dogs { get; set; } = [];
}
class Dog
{
    public int Id { get; set; }
    public DateOnly DOB { get; set; }
    public Person Owner { get; set; } = null!;
}

class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsUnicode();
    }
}