namespace Backend.Models.Context.Site
{
    public record Site(int Id, int ProviderId, string DomainName, string? YandexCounterKey);
}
