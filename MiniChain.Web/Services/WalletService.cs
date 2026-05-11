using System;
using System.Collections.Generic;
using System.IO;
using MiniChain.Core.Interface;
using MiniChain.Core.Services;

namespace MiniChain.Web.Services;

public class WalletService
{
    private readonly Dictionary<string, Wallet> _wallets = new();
    
    public IWallet? GetWallet(string? userId) => 
        userId is null ? null : _wallets.TryGetValue(userId!, out var wallet) ? wallet : null;

    public void SetWallet(string userId, IWallet wallet)
    {
        _wallets[userId] = (Wallet) wallet;
        wallet.SaveWallet(WalletPath(userId));
    }

    public void LoadAll()
    {
        foreach (var path in Directory.GetFiles(".", "wallet_*.json"))
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var userId = Uri.UnescapeDataString(fileName["wallet_".Length..]);
            _wallets[userId] = (Wallet.LoadWallet(path) as Wallet)!;
        }
    }

    private static string WalletPath(string userId) =>
        $"wallet_{Uri.EscapeDataString(userId)}.json";
}