using Microsoft.EntityFrameworkCore;

public class CarRepository : BaseRepository<Car>
{
    public CarRepository(ApplicationDbContext db) : base(db) { }

    // Fetch car with images
    public async Task<Car?> GetByIdWithImagesAsync(Guid id)
    {
        return await _set
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<IEnumerable<Car>> GetAllAsync()
    {
        return await _set
            .Include(c => c.Images) // Include the related images
            .ToListAsync();         // Retrieve all cars as a list
    }
}
