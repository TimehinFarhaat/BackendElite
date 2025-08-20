public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public CarRepository Cars { get; }
    public IBaseRepository<CarImage> CarImages { get; }
    public IBaseRepository<Inquiry> Inquiries { get; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Cars = new CarRepository(_db);         // properly initialize CarRepository
        CarImages = new BaseRepository<CarImage>(_db);
        Inquiries = new BaseRepository<Inquiry>(_db);
    }

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
    public void Dispose() => _db.Dispose();
}
