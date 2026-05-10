using MiniChain.Core.Services;

namespace MiniChain.Tests;

public class BlockTests
{
    [Fact]
    public void Genesis_HashIsReproducibleAcrossCalls()
    {
        var hash1 = Block.CreateGenesis().ComputeHash();
        var hash2 = Block.CreateGenesis().ComputeHash();
        
        Assert.Equal(hash1, hash2);
    }
}
