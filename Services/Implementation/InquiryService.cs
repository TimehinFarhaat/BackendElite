public class InquiryService : IInquiryService
{
    private readonly IUnitOfWork _uow;

    public InquiryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<InquiryDto>> GetAllInquiriesAsync()
    {
        var inquiries = await _uow.Inquiries.GetAllAsync();

        var result = new List<InquiryDto>();

        foreach (var i in inquiries)
        {
            var car = await _uow.Cars.GetByIdAsync(i.CarId); // fetch car by ID

            result.Add(new InquiryDto
            {
                Id = i.Id,
                CarId = i.CarId,
                carMaker = car?.Make ?? "Unknown",   // safe null access
                carModel = car?.Model ?? "Unknown",  // safe null access
                Name = i.Name,
                Email = i.Email,
                Message = i.Message,
                CreatedAt = i.CreatedAt,
                Response = string.IsNullOrWhiteSpace(i.Response) ? "No response yet" : i.Response
            });
        }

        return result;
    }


    public async Task<IEnumerable<InquiryDto>> GetInquiriesByEmailAsync(string email)
    {
        var inquiries = await _uow.Inquiries.FindAsync(i => i.Email.ToLower() == email.ToLower());

        var result = new List<InquiryDto>();

        foreach (var i in inquiries)
        {
            var car = await _uow.Cars.GetByIdAsync(i.CarId); // fetch car by ID

            result.Add(new InquiryDto
            {
                Id = i.Id,
                CarId = i.CarId,
                carMaker = car?.Make ?? "Unknown",
                carModel = car?.Model ?? "Unknown",
                Name = i.Name,
                Email = i.Email,
                Message = i.Message,
                Response = string.IsNullOrWhiteSpace(i.Response) ? "No response yet" : i.Response,
                CreatedAt = i.CreatedAt
            });
        }
        return result;
    }
    public async Task<InquiryDto> GetInquiryByIdAsync(Guid id)
    {
        // Fetch the inquiry by ID
        var inquiry = await _uow.Inquiries.GetByIdAsync(id); // Assuming your repository has GetByIdAsync
        if (inquiry == null)
            throw new KeyNotFoundException($"Inquiry with ID {id} not found.");

        // Map to DTO
        return new InquiryDto
        {
            Id = inquiry.Id,
            Name = inquiry.Name,
            Email = inquiry.Email,
            Message = inquiry.Message,
            Response = string.IsNullOrWhiteSpace(inquiry.Response) ? "No response yet" : inquiry.Response,
            CreatedAt = inquiry.CreatedAt
        };

    }

    public async Task<InquiryDto> CreateInquiryAsync(CreateInquiryDto inquiryDto)
    {
        var inquiry = new Inquiry
        {
            CarId = inquiryDto.CarId,
            Name = inquiryDto.Name,
            Email = inquiryDto.Email,
            Message = inquiryDto.Message,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Inquiries.AddAsync(inquiry);
        await _uow.SaveChangesAsync();

        return new InquiryDto
        {
            Id = inquiry.Id,
            CarId = inquiry.CarId,
            Name = inquiry.Name,
            Email = inquiry.Email,
            Message = inquiry.Message,
            CreatedAt = inquiry.CreatedAt
        };
    }


    public async Task<InquiryDto> UpdateUserInquiryAsync(Guid id, UpdateInquiryDto updateDto)
    {
        var inquiry = await _uow.Inquiries.GetByIdAsync(id);
        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found");

        // Only allow user to update these fields, NOT Response
        inquiry.Name = updateDto.Name ?? inquiry.Name;
        inquiry.Email = updateDto.Email ?? inquiry.Email;
        inquiry.Message = updateDto.Message ?? inquiry.Message;

        // Ignore Response update here

        await _uow.SaveChangesAsync();

        return new InquiryDto
        {
            Id = inquiry.Id,
            CarId = inquiry.CarId,
            Name = inquiry.Name,
            Email = inquiry.Email,
            Message = inquiry.Message,
            Response = string.IsNullOrWhiteSpace(inquiry.Response) ? "No response yet" : inquiry.Response,
            CreatedAt = inquiry.CreatedAt
        };

    }


    public async Task<ResponseInquiryDto> ReplyToInquiryAsync(Guid id, string response)
    {
        var inquiry = await _uow.Inquiries.GetByIdAsync(id);
        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found");

        inquiry.Response = response;

        await _uow.SaveChangesAsync();

        return new ResponseInquiryDto
        {
            Id = inquiry.Id,
            CarId = inquiry.CarId,
            Name = inquiry.Name,
            Email = inquiry.Email,
            Message = inquiry.Message,
            Response = inquiry.Response,
            CreatedAt = inquiry.CreatedAt
        };
    }

    public async Task<ResponseInquiryDto> UpdateInquiryResponseAsync(Guid id, string response)
    {
        // 1️⃣ Get the inquiry from the database
        var inquiry = await _uow.Inquiries.GetByIdAsync(id);
        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found");

        // 2️⃣ If response is null/empty, keep existing response or default
        if (!string.IsNullOrWhiteSpace(response))
        {
            inquiry.Response = response; // Replace previous response
        }
        else if (string.IsNullOrWhiteSpace(inquiry.Response))
        {
            inquiry.Response = "No response yet"; // default if previously empty
        }

        // 3️⃣ Save changes
        await _uow.SaveChangesAsync();

        // 4️⃣ Return the DTO
        return new ResponseInquiryDto
        {
            Id = inquiry.Id,
            CarId = inquiry.CarId,
            Name = inquiry.Name,
            Email = inquiry.Email,
            Message = inquiry.Message,
            Response = inquiry.Response,
            CreatedAt = inquiry.CreatedAt
        };
    }


    public async Task DeleteInquiryAsync(Guid inquiryId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can delete inquiries.");

        var inquiry = await _uow.Inquiries.GetByIdAsync(inquiryId)
            ?? throw new KeyNotFoundException("Inquiry not found.");

        if (string.IsNullOrWhiteSpace(inquiry.Response))
            throw new InvalidOperationException("Cannot delete — inquiry has no response yet.");

        _uow.Inquiries.Remove(inquiry);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteUserInquiryAsync(Guid inquiryId, string userEmail)
    {
        var inquiry = await _uow.Inquiries.GetByIdAsync(inquiryId);
        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found.");

        if (!string.Equals(inquiry.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("You can only delete your own inquiries.");

        if (!string.IsNullOrWhiteSpace(inquiry.Response))
            throw new InvalidOperationException("You cannot delete an inquiry that already has a response.");

        _uow.Inquiries.Remove(inquiry);
        await _uow.SaveChangesAsync();
    }

    public async Task<ResponseInquiryDto> DeleteInquiryResponseAsync(Guid inquiryId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admin can delete inquiry responses.");

        var inquiry = await _uow.Inquiries.GetByIdAsync(inquiryId);
        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found.");

        inquiry.Response = null; // or string.Empty

        await _uow.SaveChangesAsync();

        return new ResponseInquiryDto
        {
            Id = inquiry.Id,
            CarId = inquiry.CarId,
            Name = inquiry.Name,
            Email = inquiry.Email,
            Message = inquiry.Message,
            Response = inquiry.Response,
            CreatedAt = inquiry.CreatedAt
        };
    }


}