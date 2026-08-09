using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using OplusDigestUtils.Models;
using QcomImageUtils.Models;

namespace OplusDigestUtils.App;


[JsonSourceGenerationOptions(
    WriteIndented = true,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(OplusDigestPartition))]
[JsonSerializable(typeof(OplusDigestPartition[]))]
[JsonSerializable(typeof(OplusDigestHeader))]
[JsonSerializable(typeof(OplusDigest))]
[JsonSerializable(typeof(OplusDigestParseResult))]
[JsonSerializable(typeof(OplusDigestParseResult[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
    public static AppJsonSerializerContext Unicode { get; } = new(new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    });
}
