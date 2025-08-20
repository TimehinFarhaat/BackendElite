using System.ComponentModel.DataAnnotations;

public class Car
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Car make is required.")]
    [StringLength(50, ErrorMessage = "Make cannot be longer than 50 characters.")]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Car model is required.")]
    [StringLength(50, ErrorMessage = "Model cannot be longer than 50 characters.")]
    public string Model { get; set; } = string.Empty;

    [Range(1886, 2100, ErrorMessage = "Year must be between 1886 and 2100.")] // 1886 = first car year
    public int Year { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Price must be a positive value.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Car description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot be longer than 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    // Navigation
    public ICollection<CarImage> Images { get; set; } = new List<CarImage>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
