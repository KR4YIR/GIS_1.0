namespace WebApplication3.Token
{
    public class Clients
    {
        public List<ClientItem> Items { get; set; } = default!;
    }
    public record ClientItem(string Id, string Secret);
}
