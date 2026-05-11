using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniChain.Core.Interface;
using MiniChain.Core.Services;
using MiniChain.Web.Data;

namespace MiniChain.Web.Services;

public class ChainService(IServiceScopeFactory scopeFactory) : IHostedService
{
   public Blockchain Blockchain { get; } = new(3);
   public Mempool Mempool { get; } = new();
   public IWallet? Wallet { get; set; }
   public event Action? OnChange;
   public void NotifyStateChanged() => OnChange?.Invoke();

   public async Task StartAsync(CancellationToken cancellationToken)
   {
      using var scope = scopeFactory.CreateScope();
      var repo = scope.ServiceProvider.GetRequiredService<ChainRepository>();
      var blocks = await repo.LoadAllBlocksAsync();
      if (blocks.Count > 0)
      {
         var fullChain = new List<IBlock> { Block.CreateGenesis() };
         fullChain.AddRange(blocks);
         Blockchain.ReplaceChain(fullChain);
      }

      if (File.Exists("wallet.json"))
      {
         Wallet = MiniChain.Core.Services.Wallet.LoadWallet("wallet.json");
      }
   }

   public async Task MineBlockAsync(string minerAddress)
   {
      var block = await Task.Run(() =>
         Blockchain.MineFromMempool(Mempool, Mempool.Pending.Count, minerAddress));
      using var scope = scopeFactory.CreateScope();
      var repo = scope.ServiceProvider.GetRequiredService<ChainRepository>();
      await repo.SaveBlockAsync(block);
      
      NotifyStateChanged();
   }

   public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}