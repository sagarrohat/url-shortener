using Domain;

namespace Api;

public interface IUrlRepository
{
    Task CreateAsync(Url url);

    Task<Url?> GetByShortenUrlAsync(string shortenUrl);
}