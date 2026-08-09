namespace OplusDigestUtils.Models;

public class OplusDigestPartition
{
    public string Label { get; internal set; } = string.Empty;
    public string FileName { get; internal set; } = string.Empty;
    public bool AllowRead { get; internal set; }
    public bool AllowWrite { get; internal set; }
    public ulong StartSector { get; internal set; }
    public ulong Sectors { get; internal set; }
    public string HashHex { get; internal set; } = string.Empty;
}