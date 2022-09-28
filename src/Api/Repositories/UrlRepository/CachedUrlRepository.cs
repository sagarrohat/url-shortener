using Domain;
using Microsoft.Extensions.Caching.Distributed;

namespace Api;

public class CachedUrlRepository : IUrlRepository
{
    private readonly IUrlRepository _fallbackRepository;
    private readonly IDistributedCache _distributedCache;

    public CachedUrlRepository(IUrlRepository fallbackRepository, IDistributedCache distributedCache)
    {
        _fallbackRepository = fallbackRepository;
        _distributedCache = distributedCache;
    }

    public async Task CreateAsync(Url url)
    {
        await _fallbackRepository.CreateAsync(url);
    }

    public async Task<Url?> GetByShortenUrlAsync(string shortenUrl)
    {
        var url = await _distributedCache.GetRecordAsync<Url>(shortenUrl);

        if (url is not null) return url;

        url = await _fallbackRepository.GetByShortenUrlAsync(shortenUrl);

        await _distributedCache.SetRecordAsync(shortenUrl, url);

        return url;
    }
}