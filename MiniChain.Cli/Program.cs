using MiniChain.Core.Services;
using System.Diagnostics;
using System.Reflection;
using MiniChain.Core.Interface;

namespace MiniChain.Cli;

public class Program
{
    private static void ReplaceBlockAt(Blockchain chain, int index, IBlock replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<IBlock>)field!.GetValue(chain)!;
        list[index] = replacement;
    }

    private static Transaction SignedTx(Wallet sender, Wallet to, decimal amount)
    {
        var tx = new Transaction(sender.PublicKeyHex, to.PublicKeyHex, amount);
        tx.Sign(sender);
        return tx;
    }

    private static void MineFromMempoolAndPrint(Blockchain blockchain, Mempool mempool)
    {
        Console.WriteLine($"Mining block {blockchain.Height + 1}...");
        var sw = Stopwatch.StartNew();
        var block = blockchain.MineFromMempool(mempool, 2);
        sw.Stop();
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Mini Chain Demo");
        
        var mempool = new Mempool();
        var alice = new Wallet();
        var bob = new Wallet();
        
        Console.WriteLine($"Alice: {alice.PublicKeyHex[..12]}...");
        Console.WriteLine($"Bob: {bob.PublicKeyHex[..12]}...");

        var chain = new Blockchain(4);
        mempool.Submit(SignedTx(alice, bob, 10));
        mempool.Submit(SignedTx(bob, alice, 6));
        mempool.Submit(SignedTx(bob, alice, 100));
        Console.WriteLine($"Mempool has {mempool.Pending.Count} transactions");
        MineFromMempoolAndPrint(chain, mempool);
        Console.WriteLine($"Mempool has {mempool.Pending.Count} transactions");

        foreach (var block in chain.Blocks)
        {
            Console.WriteLine($"{block}");
        }
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");

        Console.WriteLine("Simulating Tampering....");

        var chain1 = new Blockchain(4);
        mempool.Submit(SignedTx(alice, bob, 102));
        mempool.Submit(SignedTx(bob, alice, 200));
        mempool.Submit(SignedTx(bob, alice, 200));
        MineFromMempoolAndPrint(chain1, mempool);
        MineFromMempoolAndPrint(chain1, mempool);
        
        ReplaceBlockAt(chain, 1, chain1.Blocks[2]);

        Console.WriteLine($"The chain is valid:{chain.IsValid()}");
    }
}