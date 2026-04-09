namespace Backend.Models.Context.Site.Contracts
{
    public record CreateSiteRequest(int ProviderId, string DomainName, string? YandexCounterKey);
}
