namespace Backend.Application.Contracts.Sites
{
    public record UpdateSiteRequest(int Id, string? DomainName, string? YandexCounterKey);
}