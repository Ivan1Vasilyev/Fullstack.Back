namespace Backend.Domain.Models
{
    public record Site(int Id, int ProviderId, string DomainName, string? YandexCounterKey);
}
