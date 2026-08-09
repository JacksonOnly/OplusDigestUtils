using System.Text;
using System.Text.Json;
using OplusDigestUtils;
using OplusDigestUtils.App;
using OplusDigestUtils.Models;

Console.OutputEncoding = Encoding.UTF8;
bool json = false;
var paths = new List<string>();


for (int index = 0; index < args.Length; index++)
{
    string argument = args[index];
    switch (argument)
    {
        case "--json":
            json = true;
            break;
        case "--help":
        case "-h":
            PrintUsage();
            return 0;
        default:
            if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"未知选项: {argument}");
                PrintUsage();
                return 2;
            }
            paths.Add(argument);
            break;
    }
}

if (paths.Count == 0)
{
    PrintUsage();
    return 2;
}

return ParseDigests(paths, json);

static int ParseDigests(
    IReadOnlyList<string> paths,
    bool json)
{
    var parser = new OplusDigestParser();
    var results = new OplusDigestParseResult[paths.Count];
    bool allSucceeded = true;
    for (int index = 0; index < paths.Count; index++)
    {
        bool success = parser.TryParse(paths[index], out OplusDigestParseResult result);
        results[index] = result;
        allSucceeded &= success;
    }
    if (json)
    {
        if (results.Length == 1)
            Console.WriteLine(JsonSerializer.Serialize(
                results[0],
                AppJsonSerializerContext.Unicode.OplusDigestParseResult));
        else
            Console.WriteLine(JsonSerializer.Serialize(
                results,
                AppJsonSerializerContext.Unicode.OplusDigestParseResultArray));
    }
    else
    {
        for (int index = 0; index < results.Length; index++)
        {
            if (index > 0)
                Console.WriteLine();
            PrintResult(results[index]);
        }
    }

    return allSucceeded ? 0 : 1;
}

static void PrintResult(OplusDigestParseResult result)
{
    Console.WriteLine("Parse Result:");
    Console.WriteLine($"  IsSuccess: {result.IsSuccess}");
    Console.WriteLine($"  OemType: {result.OemType}");
    Console.WriteLine($"  SocType: {result.SocType}");
    Console.WriteLine($"  VerificationStatus: {result.VerificationStatus}");
    Console.WriteLine($"  RootCaHashHex: {result.RootCaHashHex}");

    if (!string.IsNullOrEmpty(result.ErrorMessage))
        Console.WriteLine($"  ErrorMessage: {result.ErrorMessage}");

    if (result.Digest != null)
    {
        Console.WriteLine("  Digest:");
        var header = result.Digest.Header;
        Console.WriteLine("    Header:");
        Console.WriteLine($"      Magic: 0x{header.Magic:X8}");
        Console.WriteLine($"      Version: {header.Version}");
        Console.WriteLine($"      HeaderSize: {header.HeaderSize}");
        Console.WriteLine($"      PartitionNums: {header.PartitionNums}");
        Console.WriteLine($"      PartitionTotalLength: {header.PartitionTotalLength}");
        Console.WriteLine($"      PartitionLength: {header.PartitionLength}");
        Console.WriteLine($"      Reserved: {header.Reserved}");

        var partitions = result.Digest.Partitions;
        if (partitions != null && partitions.Count > 0)
        {
            Console.WriteLine("    Partitions:");
            for (int i = 0; i < partitions.Count; i++)
            {
                var p = partitions[i];
                Console.WriteLine($"      [{i}]:");
                Console.WriteLine($"        Label: {p.Label}");
                Console.WriteLine($"        FileName: {p.FileName}");
                Console.WriteLine($"        AllowRead: {p.AllowRead}");
                Console.WriteLine($"        AllowWrite: {p.AllowWrite}");
                Console.WriteLine($"        StartSector: {p.StartSector}");
                Console.WriteLine($"        Sectors: {p.Sectors}");
                Console.WriteLine($"        HashHex: {p.HashHex}");
            }
        }
        else
        {
            Console.WriteLine("    Partitions: (none)");
        }
    }
    else
    {
        Console.WriteLine("  Digest: null");
    }
}
static void PrintUsage()
{
    Console.WriteLine(
        "OplusDigestUtils <镜像或目录路径> [更多路径] [--json]");
}
