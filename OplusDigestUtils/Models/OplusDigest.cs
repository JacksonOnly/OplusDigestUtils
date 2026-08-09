namespace OplusDigestUtils.Models;

public class OplusDigest
{
    public OplusDigestHeader Header { get; internal set; } = null!;
    public IReadOnlyList<OplusDigestPartition> Partitions { get; internal set; } = [];
}
