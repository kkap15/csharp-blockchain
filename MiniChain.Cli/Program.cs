using MiniChain.Core.Services;
using System.Diagnostics;
using System.Reflection;
using MiniChain.Core.Interface;

public class Program
{
    private static void ReplaceBlockAt(Blockchain chain, int index, IBlock replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<IBlock>) field!.GetValue(chain)!;
        list[index] = replacement;
    }
    
    private static Transaction SignedTx(Wallet sender, Wallet to, decimal amount)
    {
        var tx = new Transaction(sender.PublicKeyHex, to.PublicKeyHex, amount);
        tx.Sign(sender);
        return tx;
    }
    
    private static IBlock MineAndPrint(Blockchain blockchain, IReadOnlyList<ITransaction> transactions)
    {
        Console.WriteLine($"Mining block {blockchain.Height + 1}...");
        var sw = Stopwatch.StartNew();
        var block = blockchain.AddBlock(transactions);
        sw.Stop();
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        
        return block; 
    }
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Mini Chain Demo");
        
        var alice = new Wallet();
        var bob = new Wallet();
        Console.WriteLine($"Alice: {alice.PublicKeyHex[..12]}...");
        Console.WriteLine($"Bob: {bob.PublicKeyHex[..12]}...");

        var chain = new Blockchain(4);
        MineAndPrint(chain, [SignedTx(alice, bob, 10)]);
        MineAndPrint(chain, [SignedTx(bob, alice, 6)]);
        MineAndPrint(chain, [SignedTx(bob, alice, 100)]);
        
        foreach (var block in chain.Blocks)
        {
            Console.WriteLine($"{block}");
        }
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");

        Console.WriteLine("Simulating Tampering....");

        var chain1 = new Blockchain(4);
        MineAndPrint(chain1, [SignedTx(alice, bob, 102)]);
        MineAndPrint(chain1, [SignedTx(bob, alice, 200)]);
        
        ReplaceBlockAt(chain, 1, chain1.Blocks[1]);
        ReplaceBlockAt(chain, 2, chain1.Blocks[2]);
        
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");
    }
}