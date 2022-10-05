namespace Api;

public class GenerateRequest
{
    public string Url { get; set; } = null!;
    public DateTime? Expiry { get; set; } = null;
}