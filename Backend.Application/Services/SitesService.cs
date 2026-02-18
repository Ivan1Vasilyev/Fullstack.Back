using Backend.Application.Contracts.Sites;
using Backend.Application.Exceptions;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Common.Exceptions;
using Backend.Domain.Models;
using System.Text.RegularExpressions;

namespace Backend.Application.Services
{
    public partial class SitesService(ISitesRepository sitesRepository) : ISitesService
    {

        public async Task<SiteDto> CreateAsync(CreateSiteRequest request)
        {
            var errorMessages = new List<string>();

            var domain = request.DomainName;
            if (!IsValidDomainStrict(domain, out var errorMessage))
                errorMessages.Add(errorMessage);

            var providerId = request.ProviderId;
            if (providerId < 1)
                errorMessages.Add("Id провайдера должен быть больше 0");

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            try
            {
                var createdSite = await sitesRepository.CreateAsync(providerId, domain, request.YandexCounterKey)
                    ?? throw new InfrastructureException("Сайт не был создан");

                return MapToSiteDto(createdSite);
            }
            catch (UniqueViolationException ex)
            {
                throw new ConflictException(ex.Message);
            }
            catch (ForeignKeyException ex)
            {
                throw new ConflictException(ex.Message);
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<IEnumerable<SiteDto>> GetByProviderIdAsync(int providerId)
        {
            if (providerId < 1)
                throw new ValidationException("id провайдера должен быть больше 0");

            try
            {
                var sites = await sitesRepository.GetByProviderIdAsync(providerId);
                return sites.Select(MapToSiteDto);
            }
            catch (Exception exception)
            {
                throw new InfrastructureException(exception.Message);
            }
        }

        public async Task<SiteDto> UpdateAsync(UpdateSiteRequest request)
        {
            var id = request.Id;
            if (id < 1)
                throw new ValidationException("id должен быть больше 0");

            var domain = request.DomainName;
            if (!IsValidDomainStrict(domain, out var errorMessage))
                throw new ValidationException(errorMessage);

            try
            {
                var updatedSite = await sitesRepository.UpdateAsync(id, domain, request.YandexCounterKey)
                    ?? throw new InfrastructureException("Сайте не был обновлён");

                return MapToSiteDto(updatedSite);
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

        private SiteDto MapToSiteDto(Site site) => new(site.Id, site.ProviderId, site.DomainName, site.YandexCounterKey);

        private static bool IsValidDomainStrict(string? domain, out string ErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                ErrorMessage = "домен не может быть null или пустой строкой";
                return false;
            }

            domain = domain.Trim().ToLower()
                           .Replace("http://", "")
                           .Replace("https://", "")
                           .Replace("www.", "");

            int slashIndex = domain.IndexOf('/');
            if (slashIndex >= 0)
                domain = domain[..slashIndex];

            if (domain.Length > 253)
            {
                ErrorMessage = "Длина домена не должна превышать 253 символа";
                return false;
            }

            if (domain.Contains("..") || domain.StartsWith('.') || domain.EndsWith('.'))
            {
                ErrorMessage = "В домене где-то 2 точки подряд или точка стоит в начале или в конце";
                return false;
            }

            var parts = domain.Split('.');

            if (parts.Length < 2)
            {
                ErrorMessage = "Должна быть как минимум 1 точка";
                return false;
            }

            foreach (var part in parts)
            {
                if (part.Length > 63)
                {
                    ErrorMessage = $"Эта часть [{part}] не должна быть больше 63 символов";
                    return false;
                }

                if (part.StartsWith('-') || part.EndsWith('-'))
                {
                    ErrorMessage = $"Эта часть [{part}] начинается или заканчивается '-'";
                    return false;
                }

                if (!AllowedSymbols().IsMatch(part))
                {
                    ErrorMessage = $"Эта часть [{part}] содержит недопустимые символы";
                    return false;
                }
            }

            var tld = parts[^1];
            if (tld.Length < 2 || !TldRegex().IsMatch(tld))
            {
                ErrorMessage = "Домен верхнего уровня (TLD) некорректен. Примеры правильных TLD: .com, .ru, .org, .net";
                return false;
            }

            ErrorMessage = "";

            return true;
        }

        [GeneratedRegex(@"^[a-z]{2,}$")]
        private static partial Regex TldRegex();

        [GeneratedRegex(@"^[a-z0-9\-]+$")]
        private static partial Regex AllowedSymbols();
    }
}
