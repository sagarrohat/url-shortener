using Microsoft.Extensions.Caching.Distributed;

namespace Api;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly DatabaseContext _databaseContext;
    private readonly IDistributedCache _distributedCache;

    public RepositoryFactory(DatabaseContext databaseContext, IDistributedCache distributedCache)
    {
        _databaseContext = databaseContext;
        _distributedCache = distributedCache;
    }

    public IUrlRepository UrlRepository
    {
        get
        {
            var fallbackRepository = new UrlRepository(_databaseContext);

            return new CachedUrlRepository(fallbackRepository, _distributedCache);
        }
    }
}