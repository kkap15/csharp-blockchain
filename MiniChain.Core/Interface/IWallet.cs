namespace MiniChain.Core.Interface;

public interface IWallet
{
    public string PublicKeyHex { get; }
    public string Sign(string data);
    public bool Verify(string data, string signatureHex);
}