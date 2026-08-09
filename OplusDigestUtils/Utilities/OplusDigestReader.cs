using System.Buffers.Binary;
using System.Text;
using OplusDigestUtils.Models;

namespace OplusDigestUtils.Utilities;

internal class OplusDigestReader
{
    private const uint DIGEST_MAGIC = 0x7CEF0312;
    private const int DIGEST_HEADER_LENGTH = 32;

    public static OplusDigestPartition ReadPartition(
#if NET5_0_OR_GREATER
        ReadOnlySpan<byte> buffer
#else
        byte[] buffer
#endif
    )
    {
        var partition = new OplusDigestPartition();
#if NET5_0_OR_GREATER
        partition.Label = Encoding.ASCII.GetString(buffer.Slice(0, 0x20)).TrimEnd('\0');
        partition.FileName = Encoding.ASCII.GetString(buffer.Slice(0x20, 0x20)).TrimEnd('\0');
        partition.AllowRead = BitConverter.ToBoolean(buffer.Slice(0x40));
        partition.AllowWrite = BitConverter.ToBoolean(buffer.Slice(0x44));
        partition.StartSector = BitConverter.ToUInt64(buffer.Slice(0x54));
        partition.Sectors = BitConverter.ToUInt64(buffer.Slice(0x5C));
        partition.HashHex = Convert.ToHexString(buffer.Slice(0x64));
#else
        partition.Label = Encoding.ASCII.GetString(buffer, 0, 0x20).TrimEnd('\0');
        partition.FileName = Encoding.ASCII.GetString(buffer, 0x20, 0x20).TrimEnd('\0');
        partition.AllowRead = BitConverter.ToBoolean(buffer, 0x40);
        partition.AllowWrite = BitConverter.ToBoolean(buffer, 0x44);
        partition.StartSector = BitConverter.ToUInt64(buffer, 0x54);
        partition.Sectors = BitConverter.ToUInt64(buffer, 0x5C);
        partition.HashHex = BitConverter.ToString(buffer, 0x64).Replace("-", string.Empty);
#endif
        return partition;
    }

    public static OplusDigestHeader ReadHeader(
#if NET5_0_OR_GREATER
        ReadOnlySpan<byte> buffer
#else
        byte[] buffer
#endif
    )
    {
        var header = new OplusDigestHeader();
#if NET5_0_OR_GREATER
        header.Magic = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        header.Version = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4));
        header.HeaderSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(8));
        header.PartitionNums = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(12));
        header.PartitionTotalLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(16));
        header.PartitionLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(20));
        header.Reserved = BinaryPrimitives.ReadUInt64LittleEndian(buffer.Slice(24));
#else
        header.Magic = BitConverter.ToUInt32(buffer, 0);
        header.Version = BitConverter.ToUInt32(buffer, 4);
        header.HeaderSize = BitConverter.ToUInt32(buffer, 8);
        header.PartitionNums = BitConverter.ToUInt32(buffer, 12);
        header.PartitionTotalLength = BitConverter.ToUInt32(buffer, 16);
        header.PartitionLength = BitConverter.ToUInt32(buffer, 20);
        header.Reserved = BitConverter.ToUInt64(buffer, 24);
#endif
        return header;
    }

    public static OplusDigest ReadDigest
    (
#if NET5_0_OR_GREATER
        ReadOnlySpan<byte> buffer
#else
        byte[] buffer
#endif
    )
    {
        if (buffer.Length < DIGEST_HEADER_LENGTH)
            throw new InvalidDataException("digest buffer is too small");
        var digest = new OplusDigest();
        digest.Header = ReadHeader(buffer);
        var hdr = digest.Header;
        if (hdr.Magic != DIGEST_MAGIC)
            throw new InvalidDataException("Invalid digest header");
        var digestLength = buffer.Length;
        bool isValid = hdr.HeaderSize + hdr.PartitionTotalLength == digestLength &&
                       (hdr.PartitionNums * hdr.PartitionLength) == hdr.PartitionTotalLength;
        if (!isValid)
            throw new InvalidDataException("Invalid digest");
        int partitionLength = checked((int)hdr.PartitionLength);
        var partitions = new List<OplusDigestPartition>();
        for (int i = 0; i < hdr.PartitionNums; i++)
        {
            var partitionOffset = DIGEST_HEADER_LENGTH + i * partitionLength;
#if NET5_0_OR_GREATER
            partitions.Add(
                ReadPartition(buffer.Slice(partitionOffset, partitionLength))
            );
#else
            var partitionBuffer = new byte[partitionLength];
            Array.Copy(buffer, partitionOffset, partitionBuffer, 0, partitionLength);
            partitions.Add(ReadPartition(partitionBuffer));
#endif
        }

        digest.Partitions = partitions;

        return digest;
    }
}