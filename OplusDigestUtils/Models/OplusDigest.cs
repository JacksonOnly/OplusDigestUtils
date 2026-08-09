namespace OplusDigestUtils.Models;

public class OplusDigest
{
    public OplusDigestHeader Header { get; internal set; }
    public IReadOnlyList<OplusDigestPartition> Partitions { get; internal set; } = [];
}