namespace Backend.Models.Context.Page.Contracts
{
    public record GetByParentIdRequest(int SiteId, int? ParentId = null);
}
