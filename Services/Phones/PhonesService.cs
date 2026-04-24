using Backend.Exceptions;
using Backend.Models.Context.Phone;
using Backend.Models.Context.Phone.Contracts;
using Backend.Repositories.Phones;

namespace Backend.Services.Phones
{
    public class PhonesService(IPhonesRepository phonesRepository) : IPhonesService
    {
        public async Task<IEnumerable<Phone>> GetBySiteIdAsync(int siteId)
        {
            if (siteId < 1)
                throw new ValidationException("siteId должен быть больше 0");

            try
            {
                return await phonesRepository.GetBySiteIdAsync(siteId);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Phone> CreateAsync(CreatePhoneRequest request)
        {
            var errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Label))
                errorMessages.Add("Label не должен быть пустым");

            if (string.IsNullOrWhiteSpace(request.Link))
                errorMessages.Add("Link не должен быть пустым");

            if (string.IsNullOrWhiteSpace(request.Name))
                errorMessages.Add("Name не должен быть пустым");

            if (request.Role < 0)
                errorMessages.Add($"Role должен быть больше 0 или 0. Получено: {request.Role}");

            if (request.SiteId < 1)
                errorMessages.Add("SiteId должен быть больше 0");

            if (request.CityTagIds.Length < 1)
                errorMessages.Add("Не выбран ни один city Tag");

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            try
            {
                return await phonesRepository.CreateAsync(request)
                    ?? throw new InfrastructureException("Телефон не был создан");
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Phone> UpdateAsync(Phone request)
        {
            var errorMessages = new List<string>();

            if (request.Id < 1)
                errorMessages.Add("Id должен быть больше 0");

            if (string.IsNullOrWhiteSpace(request.Label))
                errorMessages.Add("Label не должен быть пустым");

            if (string.IsNullOrWhiteSpace(request.Link))
                errorMessages.Add("Link не должен быть пустым");

            if (string.IsNullOrWhiteSpace(request.Name))
                errorMessages.Add("Name не должен быть пустым");

            if (request.Role < 0)
                errorMessages.Add($"Role должен быть больше 0 или 0. Получено: {request.Role}");

            if (request.SiteId < 1)
                errorMessages.Add("SiteId должен быть больше 0");

            if (request.CityTagIds.Length < 1)
                errorMessages.Add("Не выбран ни один city Tag");

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            try
            {
                return await phonesRepository.UpdateAsync(request)
                    ?? throw new InfrastructureException("Телефон не был обновлён");
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id < 1)
                throw new ValidationException("Id должен быть больше 0");

            try
            {
                return await phonesRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
    }
}
