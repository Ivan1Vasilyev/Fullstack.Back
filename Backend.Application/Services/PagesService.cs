using Backend.Application.Contracts.Pages;
using Backend.Application.Exceptions;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Common.Exceptions;
using Backend.Domain.Models;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Backend.Application.Services
{
    public partial class PagesService(IPagesRepository pagesRepository) : IPagesService
    {
        public async Task<PageDto> CreateAsync(CreatePageRequest page)
        {
            var errorMessages = new List<string>();

            if (page.ParentId != null && page.ParentId < 1)
                errorMessages.Add("ParentId должен быть больше 0 или null");

            if (page.SiteId < 1)
                errorMessages.Add("SiteId должен быть больше 0");

            if (!IsValidPageAlias(page.Alias, out var errorMessage))
                errorMessages.Add(errorMessage);

            if (!string.IsNullOrEmpty(page.Content) && !IsValidJson(page.Content))
                errorMessages.Add($"Content содержит невалидный json:\n{page.Content}");

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            try
            {
                var createdPage = await pagesRepository.CreateAsync(page.Name, page.Type, page.Alias, page.SiteId, page.ParentId, page.Content, page.Title, page.Description) 
                    ?? throw new InfrastructureException("Страница не была создана");

                return MapToPageDto(createdPage);
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

        public Task<IEnumerable<PageDto>> GetSitePagesAsync(int siteId)
        {
            throw new NotImplementedException();
        }

        private static bool IsValidPageAlias(string uri, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(uri))
            {
                errorMessage = "Alias не может быть пустым";
                return false;
            }

            if (uri.Contains('/') && uri.Length > 1)
            {
                errorMessage = "Alias не может содержать \"/\" (кроме главной страницы)";
                return false;
            }

            // Проверка на недопустимые символы
            if (!AliasRegex().IsMatch(uri))
            {
                errorMessage = "Alias может содержать только a-z, 0-9, \"-\" и \"_\"";
                return false;
            }

            if (uri.Length > 200)
            {
                errorMessage = "Alias слишком длинный (максимум 200 символов)";
                return false;
            }

            return true;
        }

        [GeneratedRegex(@"^[a-z0-9\/\-_]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "ru-RU")]
        private static partial Regex AliasRegex();

        private static PageDto MapToPageDto(Page page) => new (page.Id, page.Name, page.Type,  page.Alias, page.SiteId, page.ParentId, page.Content, page.Title, page.Description);

        public Task<IEnumerable<PageDto>> GetChildPagesAsync(int parentId)
        {
            throw new NotImplementedException();
        }

        private static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                using (JsonDocument.Parse(json))
                {
                    return true;
                }
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
