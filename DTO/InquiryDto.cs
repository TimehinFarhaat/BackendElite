using System.ComponentModel.DataAnnotations;

public class InquiryDto
{
    public Guid Id { get; set; }

    [Required]
    public Guid CarId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string carMaker { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string carModel { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Message { get; set; } = string.Empty;

    
    [StringLength(5000)]
    public string? Response { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateInquiryDto
{
 

    [Required]
    public Guid CarId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class UpdateInquiryDto
{
    public Guid Id { get; set; }

    public Guid CarId { get; set; }

    [StringLength(100)]
    public string? Name { get; set; } = string.Empty;

    [EmailAddress]
    [Required(AllowEmptyStrings = false)]
    public string? Email { get; set; } = string.Empty;

  
    [StringLength(5000)]
    public string? Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class ResponseInquiryDto
{
    public Guid Id { get; set; }
    public Guid CarId { get; set; }

    [Required]
    [StringLength(100)]
    public string? Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string? Message { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Response { get; set; } = null;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LoginRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Password { get; set; } = null!;
}