namespace Backend.Models.Context.Page.Contracts
{
    public record UpdatePageRequest(int Id, string Type, string Name, string? Content, string? Meta);
}
