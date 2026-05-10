using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Miner : IMiner
{
    public string? Mine(IBlock block, int difficulty)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(difficulty);
        block.Nonce = 0;
        var targetString = new string('0', difficulty);
        string? hash;
        while (true)
        {
            hash = block.ComputeHash();
            if (hash.StartsWith(targetString))
            {
                break;
            }
            block.Nonce++;
        }
        return hash;
    }
    
    public bool MeetsTarget(string hash, int difficulty)
    {
        if (hash.StartsWith(new string('0', difficulty)))
        {
            return true;
        }
        return false;
    }
}