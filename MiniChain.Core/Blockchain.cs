namespace MiniChain.Core;

public sealed class Blockchain
{
    private readonly List<Block> _blocks;
    public IReadOnlyList<Block> Blocks => _blocks;
    public Block Tip => _blocks.Last();
    public int Height => Blocks.Count - 1;
    
    public Blockchain()
    {
        _blocks = [Block.CreateGenesis()];
    }

    public Block AddBlock(IEnumerable<string> transactions)
    {
        var tip = Tip;
        var newBlock = new Block (
            tip.Index + 1,
            DateTimeOffset.UtcNow,
            tip.ComputeHash(),
            transactions
            );
        _blocks.Add(newBlock);
        return _blocks.Last();
    }

    public bool IsValid()
    {
        if (_blocks.Count == 0)
        {
            return false;
        }

        var expectedGenesisHash = Block.CreateGenesis().ComputeHash();
        if (_blocks[0].ComputeHash() != expectedGenesisHash)
        {
            return false;
        }

        for (var i = 1; i < _blocks.Count; ++i)
        {
            var current =  _blocks[i];
            var previous =  _blocks[i - 1];

            if (current.Index != i)
            {
                return false;
            }
            if (current.PreviousHash != previous.ComputeHash()) return false;
        }
        
        return true;
    }
}