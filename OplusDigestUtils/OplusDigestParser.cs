using System.Buffers.Binary;
using OplusDigestUtils.Models;
using OplusDigestUtils.Utilities;
using QcomImageUtils;
using QcomImageUtils.Types;

namespace OplusDigestUtils;

public class OplusDigestParser : IOplusDigestParser
{
    private static byte[] _elfMagic = new byte[] { 0x7F, 0x45, 0x4C, 0x46 };
    private static byte[] _mbnMagic = new byte[] { 0x1A, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00 };
    private const int MbnOffset = 0xA8;
    private const int ElfOffset = 0xE8;

    public bool TryParse(string filePath, out OplusDigestParseResult result)
    {
        result = new OplusDigestParseResult();
        if (new FileInfo(filePath).Length > 64 * 1024)
        {
            result.ErrorMessage = "对于Digest而言，数据有点长了...";
            return false;
        }

        var data = File.ReadAllBytes(filePath);
        return TryParse(data, out result);
    }

    public bool TryParse(ReadOnlySpan<byte> data, out OplusDigestParseResult result)
    {
        result = new OplusDigestParseResult();
        var qcomImgParser = new QcomImageParser();
        if (!qcomImgParser.TryParse(data, out var qcomImageParseResult))
        {
            result.ErrorMessage = "目标文件并非正确的Digest格式";
            return false;
        }

        result.OemType = qcomImageParseResult.OemType;
        result.SocType = qcomImageParseResult.SocType;
        result.RootCaHashHex = qcomImageParseResult.RootCaHash;
        // 这里为了偷懒，我就不去解析了，直接固定offset来获取对应的程序段了
        int offset = 0;
        int digestLength = 0;
        if (data.StartsWith<byte>(_elfMagic))
        {
            offset = ElfOffset;
            digestLength = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(0x98));
        }
        else if (data.StartsWith(_mbnMagic))
        {
            offset = MbnOffset;
            digestLength = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(0x14));
        }

        if (offset + digestLength > data.Length || digestLength <= 0)
        {
            result.ErrorMessage = "Digest数据偏移或长度无效";
            return false;
        }

        var buffer = data.Slice(offset, digestLength);
        try
        {
            OplusDigest digest;
#if NET5_0_OR_GREATER
            digest = OplusDigestReader.ReadDigest(buffer);
#else
            digest = OplusDigestReader.ReadDigest(buffer.ToArray());
#endif
            result.Digest = digest;
            result.ErrorMessage = null;
            result.IsSuccess = true;

            var verifier = new QcomImageVerifier();
            if (verifier.TryVerify(data, out var verificationResult))
            {
                result.VerificationStatus = verificationResult.HashTableStatus;
            }
            else
            {
                result.VerificationStatus = QcomVerificationStatus.Invalid;
            }
            return true;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.Message;
            return false;
        }
    }
}
