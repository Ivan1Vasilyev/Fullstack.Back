namespace Backend.Models.Context.Phone
{
    public record Phone(int Id, string Label, string Link, string Name, int Role, int SiteId, int[] CityTagIds);
}
