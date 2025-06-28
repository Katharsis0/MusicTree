public class FanUserGenero
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FanUserId { get; set; }
    public FanUser FanUser { get; set; }

    public int GeneroId { get; set; }
}
