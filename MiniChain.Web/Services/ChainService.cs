using System;
using MiniChain.Core.Interface;
using MiniChain.Core.Services;

namespace MiniChain.Web.Services;

public class ChainService
{
   public Blockchain Blockchain { get; } = new Blockchain(3);
   public Mempool Mempool { get; } = new Mempool();
   public IWallet? Wallet { get; set; }
   public event Action? OnChange;
   public void NotifyStateChanged() => OnChange?.Invoke();
}