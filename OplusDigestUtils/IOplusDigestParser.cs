using OplusDigestUtils.Models;

namespace OplusDigestUtils;

public interface IOplusDigestParser
{
    bool TryParse(string filePath, out OplusDigestParseResult result);

    bool TryParse(ReadOnlySpan<byte> data, out OplusDigestParseResult result);
}