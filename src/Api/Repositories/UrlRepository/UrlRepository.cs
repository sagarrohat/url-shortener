using Domain;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class UrlRepository : IUrlRepository
{
    private readonly DatabaseContext _databaseContext;

    public UrlRepository(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task CreateAsync(Url url)
    {
        _databaseContext.Add(url);

        await _databaseContext.SaveChangesAsync();
    }

    public async Task<Url?> GetByShortenUrlAsync(string shortenUrl)
    {
        return await _databaseContext.Urls.Where(x => x.Shorten == shortenUrl).FirstOrDefaultAsync();
    }
}