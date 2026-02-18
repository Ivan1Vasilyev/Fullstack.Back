namespace Backend.Domain.Models
{
    public record Page(int Id, string Name, string Type, string Alias, int SiteId, int? ParentId, string? Content, string? Title, string? Description);
}
