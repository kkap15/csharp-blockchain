using System.Security.Cryptography;
using System.Text;
using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Wallet : IWallet
{
    private readonly ECDsa KeyPair;
    public string PublicKeyHex { get; }

    public Wallet()
    {
        KeyPair = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        PublicKeyHex = Convert.ToHexStringLower(KeyPair.ExportSubjectPublicKeyInfo());
    }
    
    public string Sign(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var signature = KeyPair.SignData(bytes, HashAlgorithmName.SHA256);
        return Convert.ToHexStringLower(signature);
    }
    
    public bool Verify(string data, string signatureHex)
    {
       using var instance = ECDsa.Create();
       var signatureBytes = Convert.FromHexString(signatureHex);
       var dataBytes = Encoding.UTF8.GetBytes(data);
       instance.ImportSubjectPublicKeyInfo(Convert.FromHexString(PublicKeyHex), out _);
       if (!instance.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256))
        {
            return false;
        }
        return true;
    }
}