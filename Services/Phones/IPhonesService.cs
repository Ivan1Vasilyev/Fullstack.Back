using Backend.Models.Context.Phone;
using Backend.Models.Context.Phone.Contracts;

namespace Backend.Services.Phones
{
    public interface IPhonesService
    {
        Task<Phone> CreateAsync(CreatePhoneRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Phone>> GetBySiteIdAsync(int siteId);
        Task<Phone> UpdateAsync(Phone request);
    }
}