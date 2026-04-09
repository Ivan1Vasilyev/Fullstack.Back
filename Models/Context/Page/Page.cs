namespace Backend.Models.Context.Page
{
    public record Page(int Id, string Name, string Type, string Url, int SiteId, int? ParentId, string? Content, string? Meta);
}
