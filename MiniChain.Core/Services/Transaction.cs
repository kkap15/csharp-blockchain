using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Transaction(string from, string to, decimal amount, string signature = "") : ITransaction
{
    public string From => from;
    public string To => to;
    public decimal Amount => amount;
    public string Signature { get; private set; } = signature;

    public string SignablePayload()
    {
        return $"{From}|{To}|{Amount.ToString(CultureInfo.InvariantCulture)}";
    }
    
    public void Sign(IWallet wallet)
    {
        Signature = wallet.Sign(SignablePayload());
    }
    
    public bool IsValid()
    {
        var dataBytes = Encoding.UTF8.GetBytes(SignablePayload());
        var signatureBytes = Convert.FromHexString(Signature);
        using var ecdsaInstance = ECDsa.Create();
        ecdsaInstance.ImportSubjectPublicKeyInfo(Convert.FromHexString(From), out _);
        if (!ecdsaInstance.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256))
        {
            return false;
        }
        return true;
    }
}