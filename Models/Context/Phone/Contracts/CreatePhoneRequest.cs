namespace Backend.Models.Context.Phone.Contracts
{
    public record CreatePhoneRequest(string Label, string Link, string Name, int Role, int SiteId, int[] CityTagIds);
}
