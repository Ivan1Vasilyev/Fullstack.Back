namespace Backend.Models.Context.Page.Contracts
{
    public record CreatePageRequest(string Name, string Type, string Url, int SiteId, int? ParentId, string? Content, string? Meta);
}
