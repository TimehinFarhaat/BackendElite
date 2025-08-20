using System.ComponentModel.DataAnnotations;

public class CarDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Car make is required.")]
    [StringLength(50, ErrorMessage = "Make cannot be longer than 50 characters.")]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Car model is required.")]
    [StringLength(50, ErrorMessage = "Model cannot be longer than 50 characters.")]
    public string Model { get; set; } = string.Empty;
    [Required]
    [Range(1886, 2100, ErrorMessage = "Year must be between 1886 and 2100.")]
    public int Year { get; set; }
    [Required]
    [Range(0, 999999999.99, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    public ICollection<CarImageDto> Images { get; set; } = new List<CarImageDto>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateCarRequest
{
    [Required(ErrorMessage = "Make is required.")]
    [StringLength(50)]
    public string Make { get; set; }

    [Required(ErrorMessage = "Model is required.")]
    [StringLength(50)]
    public string Model { get; set; }
    [Required]
    [Range(1886, 2100, ErrorMessage = "Year must be between 1886 and 2100.")]
    public int Year { get; set; }

    [Range(0, 999999999.99)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000)]
    public string Description { get; set; }

    [Required(ErrorMessage = "At least one image is required.")]
    public List<IFormFile>? Images { get; set; }
}

public class UpdateCarRequest
{
    public Guid Id { get; set; }
    [StringLength(50)]
    public string? Make { get; set; }

    [StringLength(50)]
    public string? Model { get; set; }

    [Range(1886, 2100)]
    public int? Year { get; set; }

    [Range(0, 999999999.99)]
    public decimal? Price { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }




    public IFormFile[]? Images { get; set; }
}

public class CarImageDto
{
    public Guid Id { get; set; }

    [Required]
    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    public Guid CarId { get; set; }

    public CarDto Car { get; set; }
}