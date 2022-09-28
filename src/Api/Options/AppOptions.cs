namespace Api;

public class AppOptions
{
    public const string Name = "AppOptions";

    public string DatabaseConnectionString { get; set; } = null!;
    public string RedisConnectionString { get; set; } = null!;
}