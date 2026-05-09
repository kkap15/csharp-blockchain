using System.Security.Cryptography;
using System.Text;

namespace MiniChain.Core.Utilities;

public static class HashingUtils
{
    public static string Sha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
