using Backend.Application.Contracts.Provider;
using Backend.Application.DTOs.Providers;
using Backend.Application.Exceptions;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Application.Utils;
using Backend.Domain.Common.Exceptions;
using Backend.Domain.Models;

namespace Backend.Application.Services
{
    public class ProvidersService(IProvidersRepository providerRepository) : IProvidersService
    {
        public async Task<IEnumerable<ProviderDto>> GetAllAsync()
        {
            try
            {
                var providers = await providerRepository.GetAllAsync();
                return providers.Select(MapToProviderDto);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<ProviderDto> GetByIdAsync(int id)
        {
            if (id < 1)
                throw new ValidationException("id должен быть больше 0");

            try
            {
                var provider = await providerRepository.GetByIdAsync(id);
                return MapToProviderDto(provider);
            }
            catch (EntityNotFoundException ex)
            {
                throw new NotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<ProviderDto> UpdateAsync(UpdateProviderRequest request)
        {
            if (request.ProviderId < 1)
                throw new ValidationException("id должен быть больше 0");

            var newCode = LoaderCodeGenerator.GetCode(request.NewName);

            if (string.IsNullOrWhiteSpace(newCode))
                throw new ValidationException($"code не может быть пустым. Имя: {request.NewName}");
            
            try
            {
                var updatedProvider = await providerRepository.UpdateAsync(request.ProviderId, request.NewName, newCode)
                    ?? throw new InfrastructureException("Провайдер не был обновлён");

                return MapToProviderDto(updatedProvider);
            }
            catch (UniqueViolationException ex)
            {
                throw new ConflictException(ex.Message);
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<ProviderDto> CreateAsync(CreateProviderRequest request)
        {
            var newCode = LoaderCodeGenerator.GetCode(request.Name);

            if (string.IsNullOrWhiteSpace(newCode))
                throw new ValidationException($"code не может быть пустым. Имя: {request.Name}");

            try
            {
                var createdProvider = await providerRepository.CreateAsync(request.Name, newCode)
                    ?? throw new InfrastructureException("Провайдер не был создан");

                return MapToProviderDto(createdProvider);
            }
            catch (UniqueViolationException ex)
            {
                throw new ConflictException(ex.Message);
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        private ProviderDto MapToProviderDto(Provider provider) => new(provider.Id, provider.Name, provider.Code);
    }
}
