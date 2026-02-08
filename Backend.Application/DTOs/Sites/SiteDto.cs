namespace Backend.Application.DTOs.Sites
{
    public record SiteDto(int Id, int ProviderId, string DomainName, string? YandexCounterKey);
}
