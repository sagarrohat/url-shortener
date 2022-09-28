using System.Text;
using Force.Crc32;

namespace Api;

public class HashingService : IHashingService
{
    public string Compute(string text)
    {
        var crc32Hash = Crc32Algorithm.ComputeAndWriteToEnd(Encoding.UTF8.GetBytes(text));

        return $"{crc32Hash:X}".ToLower();
    }
}