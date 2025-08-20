    public interface IInquiryService
    {
        Task<IEnumerable<InquiryDto>> GetAllInquiriesAsync();
        Task<IEnumerable<InquiryDto>> GetInquiriesByEmailAsync(string email);
        Task<InquiryDto> GetInquiryByIdAsync(Guid id);
        Task<InquiryDto> CreateInquiryAsync(CreateInquiryDto inquiryDto);
        Task<InquiryDto> UpdateUserInquiryAsync(Guid id, UpdateInquiryDto updateDto);
        Task<ResponseInquiryDto> ReplyToInquiryAsync(Guid id, string response);
        Task<ResponseInquiryDto> DeleteInquiryResponseAsync(Guid inquiryId, bool isAdmin);
        Task DeleteUserInquiryAsync(Guid inquiryId, string userEmail);
        Task DeleteInquiryAsync(Guid inquiryId, bool isAdmin);
    Task<ResponseInquiryDto> UpdateInquiryResponseAsync(Guid id, string response);


    }

