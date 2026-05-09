using System.Globalization;
using MiniChain.Core.Utilities;

namespace MiniChain.Core;

public sealed class Block
{
    public int Index { get; }
    public DateTimeOffset Timestamp { get; }
    public string PreviousHash { get; }
    public IReadOnlyList<string> Transactions { get; } 
    public long Nonce { get; set; }

    public override string ToString() 
        => $"Block #{Index} [{ComputeHash()[..12]}...] prev={PreviousHash[..12]}... tx={Transactions.Count}";
    
    public Block(int index, DateTimeOffset timestamp, string previousHash, IEnumerable<string> transactions, long nonce = 0)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(previousHash);

        Index = index;
        Timestamp = timestamp;
        PreviousHash = previousHash;
        Transactions = transactions.ToList();
        Nonce = nonce;
    }
    
    public string ComputeHash()
    {
        var raw = string.Join("|",
            Index.ToString(CultureInfo.InvariantCulture),
            Timestamp.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            PreviousHash,
            string.Join(',', Transactions),
            Nonce.ToString(CultureInfo.InvariantCulture));
        var hash = HashingUtils.Sha256Hex(raw);
        
        return hash;
    }
    
    public static Block CreateGenesis()
    {
        return new Block(0, DateTimeOffset.FromUnixTimeSeconds(1_700_000_000), new string('0', 64), ["genesis"]);
    }
}
