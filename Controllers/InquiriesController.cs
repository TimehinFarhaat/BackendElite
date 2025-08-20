using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class InquiriesController : ControllerBase
{
    private readonly IInquiryService _inquiryService;

    public InquiriesController(IInquiryService inquiryService)
    {
        _inquiryService = inquiryService;
    }

    [HttpGet("getAllInquiry")]
    [AdminOnly]
    public async Task<IActionResult> GetAll()
        => Ok(await _inquiryService.GetAllInquiriesAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var inquiry = await _inquiryService.GetInquiryByIdAsync(id);
        if (inquiry == null) return NotFound();
        return Ok(inquiry);
    }


    [HttpGet("byEmail")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email required.");

            return Ok(await _inquiryService.GetInquiriesByEmailAsync(email));
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id}/updateInquiryResponse")]
    public async Task<IActionResult> UpdateResponse(Guid id, [FromForm] string response)
    {
        try
        {
            var updated = await _inquiryService.UpdateInquiryResponseAsync(id, response);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Inquiry not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update response: " + ex.Message });
        }
    }

    [HttpPost("createInquiry")]
    public async Task<IActionResult> Create([FromForm] CreateInquiryDto inquiryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Please fill in all required fields correctly." });

        try
        {
            var result = await _inquiryService.CreateInquiryAsync(inquiryDto);

            // Return the created inquiry with 201 Created
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to submit inquiry: " + ex.Message });
        }
    }

    [HttpPut("{id:guid}/updateUserInquiry")]
    public async Task<IActionResult> UpdateUserInquiry(Guid id, [FromBody]UpdateInquiryDto updateDto)
    {
        try
        {
            var updated = await _inquiryService.UpdateUserInquiryAsync(id, updateDto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{id:guid}/replyInquiry")]
    [AdminOnly]
    public async Task<IActionResult> ReplyToInquiry(Guid id, [FromForm] string response)
    {
        try
        {
            var updated = await _inquiryService.ReplyToInquiryAsync(id, response);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{inquiryId:guid}/deleteInquiry")]
    [AdminOnly]
    public async Task<IActionResult> DeleteInquiry(Guid inquiryId)
    {
        try
        {
            await _inquiryService.DeleteInquiryAsync(inquiryId, isAdmin: true);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpDelete("{inquiryId:guid}/deleteUserInquiry")]
    public async Task<IActionResult> DeleteUserInquiry(Guid inquiryId, [FromQuery] string userEmail)
    {
        try
        {
            await _inquiryService.DeleteUserInquiryAsync(inquiryId, userEmail);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPut("{inquiryId:guid}/deleteInquiryResponse")]
    [AdminOnly]
    public async Task<IActionResult> DeleteInquiryResponse(Guid inquiryId)
    {
        try
        {
            var updatedInquiry = await _inquiryService.DeleteInquiryResponseAsync(inquiryId, isAdmin: true);
            return Ok(updatedInquiry);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    private IActionResult HandleError(Exception ex)
    {
        return ex switch
        {
            ArgumentException => BadRequest(new { message = ex.Message }),
            UnauthorizedAccessException => Unauthorized(new { message = ex.Message }),
            KeyNotFoundException => NotFound(new { message = ex.Message }),
            _ => StatusCode(500, new { message = ex.Message })
        };
    }
}