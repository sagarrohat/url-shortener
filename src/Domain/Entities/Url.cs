namespace Domain;

public class Url
{
    public string Original { get; init; } = null!;
    public string Shorten { get; init; } = null!;
    public DateTime? Expiry { get; init; } = null;

    public bool IsExpired()
    {
        return Expiry.HasValue && Expiry.Value < DateTime.UtcNow;
    }
}