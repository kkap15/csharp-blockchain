using MiniChain.Core;
using System.Reflection;

public class Program
{
    private static void ReplaceBlockAt(Blockchain chain, int index, Block replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<Block>) field!.GetValue(chain)!;
        list[index] = replacement;
    }
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Mini Chain Demo");

        var chain = new Blockchain();

        chain.AddBlock(["Bob->Alice:10"]);
        chain.AddBlock(["Bob->Alice:11"]);
        chain.AddBlock(["Bob->Alice:12"]);
        chain.AddBlock(["Alice->Bob:13"]);

        foreach (var block in chain.Blocks)
        {
            Console.WriteLine($"{block}");
        }
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");

        Console.WriteLine("Simulating Tampering....");

        var chain1 = new Blockchain();
        chain1.AddBlock(["Alice->Alarico:10"]);
        chain1.AddBlock(["Alice->Alarico:11"]);
        
        ReplaceBlockAt(chain, 2, chain1.Blocks[0]);
        ReplaceBlockAt(chain, 3, chain1.Blocks[1]);
        
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");
    }
}

