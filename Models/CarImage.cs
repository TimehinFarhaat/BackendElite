using System.ComponentModel.DataAnnotations;

public class CarImage
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Image URL is required.")]
    [Url(ErrorMessage = "Image URL must be a valid URL.")]
    public string ImageUrl { get; set; } = string.Empty;

    public Guid CarId { get; set; }
    public Car? Car { get; set; }
}