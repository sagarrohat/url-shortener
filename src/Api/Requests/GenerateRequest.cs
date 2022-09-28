namespace Api;

public class GenerateRequest
{
    public string Url { get; set; } = null!;
    public bool ShouldExpire { get; set; } = false;
    public DateTime? Expiry { get; set; } = null;
}