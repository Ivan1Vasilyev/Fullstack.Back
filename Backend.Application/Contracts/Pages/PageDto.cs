namespace Backend.Application.Contracts.Pages
{
    public record PageDto(int Id, string Name, string Type, string Alias, int SiteId, int? ParentId, string? Content, string? Title, string? Description);
}
