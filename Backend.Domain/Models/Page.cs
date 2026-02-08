namespace Backend.Domain.Models
{
    public record Page(int Id, string Alias, string Type, string Name, int? ParentId);
}
