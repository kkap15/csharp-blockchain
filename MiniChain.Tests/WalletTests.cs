using MiniChain.Core.Services;

namespace MiniChain.Tests;

public class WalletTests
{
    [Fact]
    public void ExportAndImport_PreservesPublicKey()
    {
        var wallet = new Wallet();
        var privateKeyHex = wallet.ExportPrivateKey();

        var imported = (Wallet)Wallet.ImportWalletFromPrivateKey(privateKeyHex);

        Assert.Equal(wallet.PublicKeyHex, imported.PublicKeyHex);
    }

    [Fact]
    public void ImportedWallet_CanSignAndOriginalCanVerify()
    {
        var wallet = new Wallet();
        var imported = (Wallet)Wallet.ImportWalletFromPrivateKey(wallet.ExportPrivateKey());

        var signature = imported.Sign("test payload");

        Assert.True(wallet.Verify("test payload", signature));
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesPublicKey()
    {
        var path = Path.GetTempFileName();
        try
        {
            var wallet = new Wallet();
            wallet.SaveWallet(path);

            var loaded = Wallet.LoadWallet(path);

            Assert.Equal(wallet.PublicKeyHex, loaded.PublicKeyHex);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_CanSignAndVerify()
    {
        var path = Path.GetTempFileName();
        try
        {
            var wallet = new Wallet();
            wallet.SaveWallet(path);
            var loaded = Wallet.LoadWallet(path);

            var signature = loaded.Sign("test payload");

            Assert.True(loaded.Verify("test payload", signature));
        }
        finally
        {
            File.Delete(path);
        }
    }
}