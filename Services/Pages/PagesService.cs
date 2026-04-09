using Backend.Exceptions;
using Backend.Models.Context.Page;
using Backend.Models.Context.Page.Contracts;
using Backend.Repositories.Pages;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Backend.Services.Pages
{
    public partial class PagesService(IPagesRepository pagesRepository) : IPagesService
    {
        public async Task<string> UpdateUrlAsync(UpdatePageUrlRequest request)
        {
            var errorMessages = new List<string>();
            if (request.Id < 1)
                errorMessages.Add("Id должен быть больше 0");

            if (request.ParentId == null && request.ParentId < 1)
                errorMessages.Add("ParentId должен быть больше 0 и не должен быть null");

            if (request.SiteId < 1)
                errorMessages.Add("SiteId должен быть больше 0");

            if (!IsValidPageUrl(request.Url, request.ParentId, out var errorMessage))
                errorMessages.Add(errorMessage);

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            var siblingPages = (await GetByParentIdAsync(new(request.SiteId, request.ParentId)))
                .Where(x => x.Id != request.Id);

            if (siblingPages.Any(x => x.Url == request.Url))
            {
                throw new ConflictException("page", new() {
                    { "siteId", request.SiteId },
                    { "parentId", request.ParentId },
                    { "alias", request.Url }
                });
            }

            try
            {
                return await pagesRepository.UpdateAliasAsync(request) ??
                    throw new InfrastructureException("Url не был обновлён");
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<IEnumerable<Page>> GetByParentIdAsync(GetByParentIdRequest request)
        {
            var errorMessages = new List<string>();

            if (request.SiteId < 1)
                errorMessages.Add("SiteId должен быть больше 0");

            if (request.ParentId != null && request.ParentId < 1)
                errorMessages.Add("ParentId должен быть больше 0 или null");

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            try
            {
                return await pagesRepository.GetByParentIdAsync(request);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Page> UpdateAsync(UpdatePageRequest request)
        {
            if (request.Id < 1)
                throw new ValidationException("Id должен быть больше 0");

            try
            {
                return await pagesRepository.UpdateAsync(request)
                    ?? throw new InfrastructureException("Страница не была обновлена");
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }

        }

        public async Task<Page> CreateAsync(CreatePageRequest request)
        {
            var errorMessages = new List<string>();

            if (request.ParentId != null && request.ParentId < 1)
                errorMessages.Add("ParentId должен быть больше 0 или null");

            if (request.SiteId < 1)
                errorMessages.Add("SiteId должен быть больше 0");

            if (!IsValidPageUrl(request.Url, request.ParentId, out var errorMessage))
                errorMessages.Add(errorMessage);

            if (errorMessages.Count > 0)
                throw new ValidationException(errorMessages);

            var exists = await pagesRepository.PageExistsAsync(request.SiteId, request.ParentId, request.Url);
            if (exists)
            {
                throw new ConflictException("page", new() {
                    { "siteId", request.SiteId },
                    { "parentId", request.ParentId },
                    { "alias", request.Url }
                });
            }

            try
            {
                return await pagesRepository.CreateAsync(request)
                    ?? throw new InfrastructureException("Страница не была создана");
            }
            catch (Exception ex) when (ex is not InfrastructureException)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        private static bool IsValidPageUrl(string url, int? parentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            var isEmpty = string.IsNullOrWhiteSpace(url);
            var isMain = parentId == null;

            if (isEmpty && !isMain)
            {
                errorMessage = "URL может быть пустым только для главной страницы";
                return false;
            }

            if (!isEmpty && isMain)
            {
                errorMessage = "Для главной страницы URL должен быть пустым";
                return false;
            }

            if (url == "/")
            {
                errorMessage = "URL не может содержать только \"/\" ";
                return false;
            }

            var splittedUrl = url.Split('/');
            if (splittedUrl.Any(string.IsNullOrWhiteSpace))
            {
                errorMessage = $"Недопустимый URL";
                return false;
            }

            if (splittedUrl.Any(x => !AliasRegex().IsMatch(x)))
            {
                errorMessage = "URL может содержать только a-z, 0-9, \"-\" и \"_\"";
                return false;
            }

            return true;
        }

        [GeneratedRegex(@"^[a-z0-9\/\-_]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex AliasRegex();

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
