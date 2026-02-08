namespace Backend.Application.Contracts.Sites
{
    public record CreateSiteRequest(int ProviderId, string DomainName, string? YandexCounterKey);
}
