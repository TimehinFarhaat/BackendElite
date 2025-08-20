using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarImage> CarImages => Set<CarImage>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>()
            .Property(c => c.Price)
            .HasColumnType("decimal(18,2)"); // SQL Server precision

        modelBuilder.Entity<Car>()
            .HasMany(c => c.Images)
            .WithOne(i => i.Car!)
            .HasForeignKey(i => i.CarId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
