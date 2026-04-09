namespace Backend.Models.Context.Page.Contracts
{
    public record class UpdatePageUrlRequest(int Id, int SiteId, int? ParentId, string Url);
}
