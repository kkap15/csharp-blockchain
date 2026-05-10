using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Mempool : IMempool
{
    private List<ITransaction> _pending = [];
    public IReadOnlyList<ITransaction> Pending => _pending;

    public void Remove(IEnumerable<ITransaction> confirmed)
    {
        _pending.RemoveAll(x => confirmed.Contains(x));
    }

    public bool Submit(ITransaction transaction)
    {
        if (transaction.IsValid())
        {
            _pending.Add(transaction);
            return true;
        }
        return false;
    }

    public IReadOnlyList<ITransaction> Take(int count)
    {
        var list = _pending.Take(count).ToList();
        
        return list;
    }
}