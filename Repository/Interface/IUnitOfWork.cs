public interface IUnitOfWork : IDisposable
{
    CarRepository Cars { get; }
    IBaseRepository<CarImage> CarImages { get; }
    IBaseRepository<Inquiry> Inquiries { get; }
    Task<int> SaveChangesAsync();
}