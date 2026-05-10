namespace MiniChain.Core.Interface;

public interface IMiner
{
    public string? Mine(IBlock block, int difficulty);
    public bool MeetsTarget(string hash, int difficulty);
}