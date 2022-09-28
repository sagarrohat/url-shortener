namespace Domain;

public class Url
{
    public string Original { get; init; } = null!;
    public string Shorten { get; init; } = null!;
    public bool ShouldExpire { get; init; } = false;
    public DateTime? Expiry { get; init; } = null;
}