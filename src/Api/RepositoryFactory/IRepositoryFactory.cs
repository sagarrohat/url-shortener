namespace Api;

public interface IRepositoryFactory
{
    IUrlRepository UrlRepository { get; }
}