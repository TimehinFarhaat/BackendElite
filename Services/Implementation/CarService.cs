
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

public class CarService : ICarService
{
    private readonly IUnitOfWork _uow;
    private readonly ClarifaiService _clarifaiService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CarService> _logger;
    private readonly CarRepository _carRepository;
    public CarService(IUnitOfWork uow, ClarifaiService clarifaiService, IWebHostEnvironment env, ILogger<CarService> logger,CarRepository carRepository)
    {
        _carRepository = carRepository;
        
        _uow = uow;
        _clarifaiService = clarifaiService;
        _env = env;
        _logger = logger;
    }

    public async Task<IEnumerable<CarDto>> GetAllCarsAsync()
    {
        var cars = await _carRepository.GetAllAsync();

        var carDtos = cars.Select(car => new CarDto
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Price = car.Price,
            Description = car.Description,
            CreatedAt = car.CreatedAt,
            Images = car.Images?.Select(img => new CarImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                CarId = img.CarId
            }).ToList() ?? new List<CarImageDto>()
        }).ToList();

        return carDtos;
    }


    public async Task<CarDto?> GetCarByIdAsync(Guid id)
    {
        var car = await _carRepository.GetByIdWithImagesAsync(id);
        if (car == null) return null;

       
        var carDto = new CarDto
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Price = car.Price,
            Description = car.Description,
            CreatedAt = car.CreatedAt,
            Images = car.Images.Select(img => new CarImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                CarId = img.CarId
            }).ToList()
        };

        return carDto;
    }


    public async Task<CarDto> CreateCarWithImageAsync(CreateCarRequest request)
    {
        if (request.Images == null || !request.Images.Any())
            throw new ArgumentException("At least one image is required.");

        var imageList = new List<CarImage>();

        foreach (var image in request.Images)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("One of the images is empty.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".apng" };
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Only .jpg, .jpeg, .png, and .apng file extensions are allowed.");
            // Copy image to memory stream for validation and saving
            using var imgStream = new MemoryStream();
            await image.CopyToAsync(imgStream);
            imgStream.Position = 0;

            // Validate image with Clarifai (custom service)
            await _clarifaiService.ValidateCarImageAsync(imgStream);

            // Reset position to save file
            imgStream.Position = 0;

            // Save image to wwwroot/images
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imgStream.CopyToAsync(fileStream);
            }

            // Add to image list for DB
            imageList.Add(new CarImage
            {
                ImageUrl = "/images/" + fileName
            });
        }

        // Save car entity with all images
        var car = new Car
        {
            Id = Guid.NewGuid(),
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Price = request.Price,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            Images = imageList
        };

        await _uow.Cars.AddAsync(car);
        await _uow.SaveChangesAsync();

        // Map to DTO
        return new CarDto
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Price = car.Price,
            Description = car.Description,
            CreatedAt = car.CreatedAt,
            Images = car.Images.Select(img => new CarImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                CarId = img.CarId
            }).ToList()
        };
    }


    public async Task<CarDto> UpdateCarWithImageAsync(Guid carId, UpdateCarRequest request, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admin can update cars.");

        // Fetch car with tracked images
        var car = await _carRepository.GetByIdWithImagesAsync(carId);
        if (car == null) throw new KeyNotFoundException("Car not found");

        // Update basic fields
        car.Make = request.Make ?? car.Make;
        car.Model = request.Model ?? car.Model;
        car.Year = request.Year ?? car.Year;
        car.Price = request.Price ?? car.Price;
        car.Description = request.Description ?? car.Description;

        // Append new images
        if (request.Images != null && request.Images.Any())
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var imageFile in request.Images)
            {
                // Save file to disk
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                await using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fs);
                }

                // Optional: validate image
                await _clarifaiService.ValidateCarImageAsync(new MemoryStream(await File.ReadAllBytesAsync(filePath)));

                // Create CarImage and link via CarId
                var carImage = new CarImage
                {
                    ImageUrl = "/images/" + fileName,
                    CarId = car.Id
                };

                await _uow.CarImages.AddAsync(carImage); // only AddAsync is needed
            }
        }

        // Save all changes
        await _uow.SaveChangesAsync();

        // Map to DTO
        return new CarDto
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Price = car.Price,
            Description = car.Description,
            CreatedAt = car.CreatedAt,
            Images = (car.Images ?? new List<CarImage>()).Select(img => new CarImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                CarId = img.CarId
            }).ToList()
        };
    }



    public async Task DeleteCarAsync(Guid carId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admin can delete cars.");

        // Load car with images
        var car = await _uow.Cars.GetByIdWithImagesAsync(carId);
        if (car == null)
            throw new KeyNotFoundException("Car not found.");

        var uploadsFolder = Path.Combine(_env.WebRootPath, "images");

        foreach (var img in car.Images.ToList()) // ToList to avoid modifying collection while iterating
        {
            var filePath = Path.Combine(uploadsFolder, Path.GetFileName(img.ImageUrl));
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    
                    // Log error but continue deleting other images
                    // _logger.LogError(ex, $"Failed to delete image file {filePath}");
                }
            }
        }

        // Remove car (cascade delete should remove images from DB if configured)
        _uow.Cars.Remove(car);
        await _uow.SaveChangesAsync();
    }


    public async Task<CarDto> DeleteCarImageAsync(Guid carId, Guid imageId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admin can delete car images.");

        // Include images
        var car = await _uow.Cars.GetByIdWithImagesAsync(carId);
        if (car == null)
            throw new KeyNotFoundException("Car not found.");

        var image = car.Images.FirstOrDefault(img => img.Id == imageId);
        if (image == null)
            throw new KeyNotFoundException("Car image not found.");

        if (car.Images.Count <= 1)
            throw new InvalidOperationException("Cannot delete the last image. Please add another image before deleting this one.");

        var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
        var filePath = Path.Combine(uploadsFolder, Path.GetFileName(image.ImageUrl));

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to delete image file: {filePath}");
            }
        }

        car.Images.Remove(image);
        await _uow.SaveChangesAsync();

        return new CarDto
        {
            Id = car.Id,
            Make = car.Make,
            Model = car.Model,
            Year = car.Year,
            Price = car.Price,
            Description = car.Description,
            CreatedAt = car.CreatedAt,
            Images = car.Images.Select(img => new CarImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                CarId = img.CarId
            }).ToList()
        };
    }






}

