using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using var db = new DogOwnersClubContext();
//await db.Database.EnsureDeletedAsync();
//await db.Database.EnsureCreatedAsync();
//for (var i = 0; i < 3; i++)
//{
//    var p = new Person { Name = new FullName { FirstName = "John", LastName = "Doe" } };
//    db.Add(p);
//}
//db.SaveChanges();
var owner = db.Owners.Where(x => x.Id == 2).AsNoTrackingWithIdentityResolution().First();
owner.Name.LastName = DateTime.Now.Ticks.ToString();
db.Owners.ExecuteDelete
while (true)
{
    try
    {
        db.SaveChanges();
        break;
    }
    catch (DbUpdateConcurrencyException ex)
    {
        foreach (var entry in ex.Entries)
        {
            //entry.Reload();
            entry.OriginalValues.SetValues(entry.GetDatabaseValues()!);
        }
    }
}

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

        optionsBuilder.UseSqlServer("Data Source=2001:67c:d74:66:5cbb:f6ff:fe9e:eefa;Database=dogs;User=sa;Password=Pa$$w0rd;Connect Timeout=10;ConnectRetryCount=0;TrustServerCertificate=true",
            o => o.MaxBatchSize(2));
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    public override int SaveChanges()
    {
        ChangeTracker.DetectChanges();
        foreach(var item in ChangeTracker.Entries().Where(x => x.State == EntityState.Modified))
        {
            if (item.Entity is IAuditEntity audit)
            {
                audit.ModifiedAt = DateTimeOffset.Now;
            }
        }
        return base.SaveChanges();
    }
}

class Person : IAuditEntity
{
    public int Id { get; set; }
    public FullName Name { get; set; } = null!;
    public ICollection<Dog> Dogs { get; set; } = [];
    public byte[] Version { get; set; } = null!;
    public DateTimeOffset ModifiedAt { get; set; }
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
        builder.Property(x => x.Id).UseHiLo();
        builder.ComplexProperty(x => x.Name);
        builder.ToTable("Owners");
        builder.Property(x => x.Version)
            .HasColumnType("timestamp")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
class DogConfiguration : IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
    }
}

class FullName
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

interface IAuditEntity
{
    DateTimeOffset ModifiedAt { get; set; }
}
