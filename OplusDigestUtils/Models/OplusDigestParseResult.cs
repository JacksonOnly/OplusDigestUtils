using QcomImageUtils.Types;

namespace OplusDigestUtils.Models;

public class OplusDigestParseResult
{
    public bool IsSuccess { get; internal set; }
    public IReadOnlyList<OplusDigestPartition> Partitions { get; internal set; } = [];
    public QualcommOemType OemType { get; internal set; }
    public QualcommSocType SocType { get; internal set; }
    public QcomVerificationStatus  VerificationStatus { get; internal set; }
    public string RootCaHashHex { get; internal set; } = string.Empty;
    public string? ErrorMessage { get; internal set; }
}