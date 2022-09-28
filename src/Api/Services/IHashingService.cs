namespace Api;

public interface IHashingService
{
    string Compute(string text);
}