using System.Threading.Tasks;

public interface ICarService
{
    Task<IEnumerable<CarDto>> GetAllCarsAsync();
    Task<CarDto?> GetCarByIdAsync(Guid id);
    Task<CarDto> CreateCarWithImageAsync(CreateCarRequest request);
    Task<CarDto> UpdateCarWithImageAsync(Guid carId, UpdateCarRequest request, bool isAdmin);
    Task DeleteCarAsync(Guid carId, bool isAdmin);
    Task<CarDto> DeleteCarImageAsync(Guid carId, Guid imageId, bool isAdmin);

}