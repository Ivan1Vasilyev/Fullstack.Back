using Backend.Application.Services;
using Backend.Exceptions;
using Backend.Models.Context.Provider;
using Backend.Models.Context.Provider.Contracts;
using Backend.Repositories.Providers;
using Backend.Utils;

namespace Backend.Services.Providers
{
    public class ProvidersService(IProvidersRepository providerRepository) : IProvidersService
    {
        public async Task<IEnumerable<Provider>> GetAllAsync()
        {
            try
            {
                return await providerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Provider> GetByIdAsync(int id)
        {
            if (id < 1)
                throw new ValidationException("id должен быть больше 0");

            try
            {
                var provider = await providerRepository.GetByIdAsync(id);
                return provider ?? throw new NotFoundException("provider", "id", id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Provider> UpdateAsync(UpdateProviderRequest request)
        {
            var errorMessages = new List<string>();

            if (request.ProviderId < 1)
                errorMessages.Add("id должен быть больше 0");

            var newCode = LoaderCodeGenerator.GetCode(request.Name);

            if (string.IsNullOrWhiteSpace(newCode))
                errorMessages.Add($"code не может быть пустым. Имя: {request.Name}");

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            try
            {
                var updatedProvider = await providerRepository.UpdateAsync(request.ProviderId, request.Name, newCode)
                    ?? throw new InfrastructureException("Провайдер не был обновлён");

                return updatedProvider;
            }
            catch (Exception ex) when (ex is ConflictException or InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Provider> CreateAsync(CreateProviderRequest request)
        {
            var newCode = LoaderCodeGenerator.GetCode(request.Name);

            if (string.IsNullOrWhiteSpace(newCode))
                throw new ValidationException($"code не может быть пустым. Имя: {request.Name}");

            try
            {
                var createdProvider = await providerRepository.CreateAsync(request.Name, newCode)
                    ?? throw new InfrastructureException("Провайдер не был создан");

                return createdProvider;
            }
            catch (Exception ex) when (ex is ConflictException or InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
    }
}
