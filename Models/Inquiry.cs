using System.ComponentModel.DataAnnotations;

public class Inquiry
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "CarId is required.")]
    public Guid CarId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be valid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(5000, ErrorMessage = "Message cannot be longer than 5000 characters.")]
    public string Message { get; set; } = string.Empty;

    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}