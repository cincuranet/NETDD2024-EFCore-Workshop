using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using var db = new DogOwnersClubContext();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();
//db.Owners.IgnoreQueryFilters().TagWithCallSite().Load();
//db.Owners.Where(x => x.Duration == new Duration(10)).Load();
//db.Owners.Where(x => EF.Property<DateTime>(x, "LastUpdated") > DateTime.Now).Load();
//var o = new Person();
//db.Add(o);
//db.Entry(o).Property<DateTime>("LastUpdated").CurrentValue = DateTime.Now;
db.Set<Dictionary<string, object>>("FooBar").Add(new Dictionary<string, object>
{
    ["Id"] = 1,
    ["Value"] = "Foo",
    ["Created"] = DateTime.Now
});

class DogOwnersClubContext : DbContext
{
    public DbSet<Person> Owners { get; set; }
    public DbSet<Dog> Dogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());

        modelBuilder.SharedTypeEntity<Dictionary<string, object>>("FooBar", b =>
        {
            b.IndexerProperty<int>("Id");
            b.IndexerProperty<string>("Value");
            b.IndexerProperty<DateTime>("Created");
        });
        modelBuilder.SharedTypeEntity<Dictionary<string, object>>("Joe", b =>
        {
            b.IndexerProperty<int>("Id");
            b.IndexerProperty<string>("Value");
            b.IndexerProperty<DateTime>("Created");
            b.HasMany("FooBars")
                .WithOne("Joe")
                .HasForeignKey("_joeId")
                .OnDelete(DeleteBehavior.Cascade);
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

//[Table("T_OWNER")]
class Person
{
    //[Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Dog> Dogs { get; set; } = [];
    private bool _active;
    internal bool Active => _active;
    public Duration Duration { get; set; } = null!;

    public void Deactivate()
    {
        if (!_active)
            throw new InvalidOperationException("Already deactivated");
        _active = false;
    }
}
class Dog
{
    public int Id { get; set; }
    public DateOnly DOB { get; set; }
    public Person Owner { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = [];
}
class Comment
{
    private Dictionary<string, object> _attributes = [];

    public int Id { get; set; }

    public object this[string name]
    {
        get => _attributes[name];
        set => _attributes[name] = value;
    }
}

class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsUnicode();
        builder.ToTable("Owners");
        builder.HasQueryFilter(x => x.Active);
        builder.Property(x => x.Duration)
            .HasConversion(new DurationConverter());
        builder.Property(x => x.Active)
            .HasField("_active")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Property<DateTime>("LastUpdated");
    }
}

class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.IndexerProperty<DateTime>("Created");
        builder.IndexerProperty<string>("Author");
        builder.IndexerProperty<string>("Text");
    }
}

record Duration(int Seconds);

class DurationConverter : ValueConverter<Duration, int>
{
    public DurationConverter()
        : base(x => x.Seconds, x => new Duration(x))
    { }
}