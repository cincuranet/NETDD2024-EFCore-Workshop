using Microsoft.EntityFrameworkCore;

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
    public string Name { get; set; } = null!;
    public ICollection<Dog> Dogs { get; set; } = [];
}
class Dog
{
    public int Id { get; set; }
    public DateOnly DOB { get; set; }
    public Person Owner { get; set; } = null!;
}