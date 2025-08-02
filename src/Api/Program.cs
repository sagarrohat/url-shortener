using Api;
using Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var appOptionsConfigurationSection = builder.Configuration.GetSection(AppOptions.Name);
var appOptions = appOptionsConfigurationSection.Get<AppOptions>();

services.Configure<AppOptions>(appOptionsConfigurationSection);

services.AddCors();


if (builder.Environment.IsDevelopment())
{
    services.AddDistributedMemoryCache();

    services.AddDbContext<DatabaseContext>(options => options.UseInMemoryDatabase("url-shortener"));
}
else
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = appOptions.RedisConnectionString;
        options.InstanceName = "UrlShortener_";
    });

    services.AddDbContext<DatabaseContext>(
        options => options.UseNpgsql(connectionString: appOptions.DatabaseConnectionString)
    );

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}

services.AddScoped<IRepositoryFactory, RepositoryFactory>();
services.AddSingleton<IHashingService, HashingService>();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    var dbContext = app.Services.GetRequiredService<DatabaseContext>();
    dbContext.Database.EnsureCreated();
}

app.MapPost("/generate",
    async (GenerateRequest generateRequest, IHashingService hashingService, IRepositoryFactory repositoryFactory) =>
    {
        var shortenUrl = hashingService.Compute(generateRequest.Url);

        var urlRepository = repositoryFactory.UrlRepository;

        var url = await urlRepository.GetByShortenUrlAsync(shortenUrl);

        if (url is null)
        {
            await urlRepository.CreateAsync(new Url()
            {
                Original = generateRequest.Url,
                Shorten = shortenUrl,
                Expiry = generateRequest.Expiry?.ToUniversalTime()
            });
        }

        return shortenUrl;
    });

app.MapGet("/r/{shortenUrl}", async (string shortenUrl, IRepositoryFactory repositoryFactory) =>
{
    var url = await repositoryFactory.UrlRepository.GetByShortenUrlAsync(shortenUrl);

    return url is null ? Results.NotFound() : Results.Redirect(url.Original);
});

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.Run();