namespace Backend.Application.Contracts.Pages
{
    public record CreatePageRequest(string Name, string Type, string Alias, int SiteId, int? ParentId, string? Content, string? Title, string? Description);
}
