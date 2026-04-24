using Backend.Models.Context.Phone;
using Backend.Models.Context.Phone.Contracts;

namespace Backend.Repositories.Phones
{
    public interface IPhonesRepository
    {
        Task<Phone?> CreateAsync(CreatePhoneRequest request);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Phone>> GetBySiteIdAsync(int siteId);
        Task<Phone?> UpdateAsync(Phone request);
    }
}