namespace Backend.Models.Context.Site
{
    public record UpdateSiteRequest(int Id, string? DomainName, string? YandexCounterKey);
}
