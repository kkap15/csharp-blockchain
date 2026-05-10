using MiniChain.Core.Services;

namespace MiniChain.Tests;

public class MempoolTests
{
    [Fact]
    public void Submit_ValidTransactions_ReturnsTrueAndAddsToPendig()
    {
        var wallet1 = new Wallet();
        var wallet2 = new Wallet();
        var mempool = new Mempool();
        
        Assert.True(mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 10)));
        Assert.Single(mempool.Pending);
    }
    
    [Fact]
    public void Submit_InvalidTransaction_ReturnsFalseAndDoesNotAdd()
    {
        var wallet1 = new Wallet();
        var mempool = new Mempool();
        
        Assert.False(mempool.Submit(new Transaction(wallet1.PublicKeyHex, "invalid-transaction", 10)));
        Assert.Empty(mempool.Pending);
    }
    
    [Fact]
    public void Take_ReturnsFirstNTransactions()
    {
        var mempool = new Mempool();
        var wallet1 = new Wallet();
        var wallet2 = new Wallet();
        mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 10));
        mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 10));
        mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 12));
        
        var transactions = mempool.Take(2);
        
        Assert.Equal(2, transactions.Count);
        Assert.Equal(3, mempool.Pending.Count);
    }
    
    [Fact]
    public void Remove_EvictsConfirmedTransactions()
    {
        var mempool = new Mempool();
        var wallet1 = new Wallet();
        var wallet2 = new Wallet();
        var transaction1 = SignedTx(wallet1, wallet2.PublicKeyHex, 10);
        var transaction2 = SignedTx(wallet1, wallet2.PublicKeyHex, 10);
        mempool.Submit(transaction1);
        mempool.Submit(transaction2);
        
        mempool.Remove([transaction1, transaction2]);
        
        Assert.Empty(mempool.Pending);
    }
    
    [Fact]
    public void MineFromMempool_MinersBlockAndClearsMempool()
    {
        var mempool = new Mempool();
        var wallet1 = new Wallet();
        var wallet2 = new Wallet();
        mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 10));
        mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 10));
        mempool.Submit(SignedTx(wallet1, wallet2.PublicKeyHex, 12));
        
        var blockchain = new Blockchain();
        blockchain.MineFromMempool(mempool, 2);
        
        Assert.Equal(1, blockchain.Height);
        Assert.Single(mempool.Pending);
    }
    
    private static Transaction SignedTx(Wallet sender, string to, decimal amount)
    {
        var tx = new Transaction(sender.PublicKeyHex, to, amount);
        tx.Sign(sender);
        return tx;
    }
}