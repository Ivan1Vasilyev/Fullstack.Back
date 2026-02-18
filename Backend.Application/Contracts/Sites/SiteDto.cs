namespace Backend.Application.Contracts.Sites
{
    public record SiteDto(int Id, int ProviderId, string DomainName, string? YandexCounterKey);
}
