namespace OplusDigestUtils.Models;

public class OplusDigestHeader
{
    public uint Magic { get; internal set; }
    public uint Version { get; internal set; }
    public uint HeaderSize { get; internal set; }
    public uint PartitionNums { get; internal set; }
    public uint PartitionTotalLength { get; internal set; }
    public uint PartitionLength { get; internal set; }
    public ulong Reserved { get; internal set; }
}