# OplusDigestUtils

[![CI and Release](https://github.com/JacksonOnly/OplusDigestUtils/actions/workflows/publish-aot.yml/badge.svg)](https://github.com/JacksonOnly/OplusDigestUtils/actions/workflows/publish-aot.yml)
[![NuGet version](https://img.shields.io/nuget/vpre/OplusDigestUtils.svg?style=flat-square)](https://www.nuget.org/packages/OplusDigestUtils/)
[![License](https://img.shields.io/github/license/JacksonOnly/OplusDigestUtils?style=flat-square)](LICENSE)

OplusDigestUtils 是一个只读的 .NET 工具，用于解析 OPPO、realme 和 OnePlus 固件中的 Oplus 自定义 Digest MBN/ELF。它可以读取 Digest 头和分区记录，并借助 [QcomImageUtils](https://github.com/JacksonOnly/QualcommImageUtils) 识别 OEM、SoC、Root CA 哈希以及封装镜像的哈希表验证状态。

仓库包含类库和 Windows 命令行工具，既可以集成到 .NET 项目中，也可以直接解析一个或多个 Digest 文件。

## 功能

- 解析 Oplus 自定义 Digest 的头部和分区表。
- 提取分区标签、文件名、读写权限、起始扇区、扇区数量和哈希值。
- 识别 Qualcomm 镜像中的 OEM、SoC 和 Root CA 哈希。
- 返回封装镜像的 Qualcomm 哈希表验证状态。
- 提供文件路径和 `ReadOnlySpan<byte>` 两种类库 API。
- CLI 支持多个文件、普通文本输出和 JSON 输出。
- 支持 `netstandard2.0` 和 `net10.0`，并提供 Windows Native AOT 发布配置。

## 支持范围与限制

- 仅支持包含 Oplus 自定义 Digest 数据的 MBN/ELF，不支持仅包含 Qualcomm 标准哈希表的普通 Digest 格式。
- 当前实现按 Oplus Digest 的固定 MBN/ELF 布局定位数据，采用其他偏移或封装方式的变体可能无法解析。
- CLI 和 `TryParse(string, ...)` 只接受文件，不支持目录或固件包自动解包；请先从固件包中提取 Digest 文件。
- CLI 和文件路径 API 会拒绝大于 64 KiB 的输入。`ReadOnlySpan<byte>` 重载不执行这项额外的 64 KiB 预检查。
- `HashHex` 是从 Digest 分区记录中读取的值。本项目不会加载对应的分区镜像，也不会重新计算并比对这些分区哈希。
- `VerificationStatus` 表示封装 Qualcomm 镜像的哈希表状态，不等同于分区镜像验证结果，也不代表固件一定能够安全刷写或启动。
- 文件不存在、无法访问等 I/O 错误目前可能直接抛出异常；调用文件 API 前应确保路径指向可读的普通文件。

## 项目组成

| 项目 | 目标框架 | 用途 |
| --- | --- | --- |
| `OplusDigestUtils` | `netstandard2.0`、`net10.0` | 可打包的解析类库 |
| `OplusDigestUtils.App` | `net10.0-windows` | 支持 Native AOT 的 Windows CLI |

## 获取方式

### NuGet 类库

CI 成功发布类库后，可通过 NuGet 安装。版本号由日期和提交哈希组成，并带有预发布标识，因此安装时需要允许预发布版本：

```powershell
dotnet add package OplusDigestUtils --prerelease
```

如果需要直接引用源码项目：

```xml
<ItemGroup>
  <ProjectReference Include="../OplusDigestUtils/OplusDigestUtils.csproj" />
</ItemGroup>
```

### Windows CLI

CI 成功发布 CLI 后，可从 [GitHub Releases](https://github.com/JacksonOnly/OplusDigestUtils/releases) 下载与系统架构匹配的自包含 Native AOT 可执行文件：

- `win-x64`：64 位 Intel/AMD Windows
- `win-x86`：32 位 Intel/AMD Windows
- `win-arm64`：ARM64 Windows

发布文件名包含运行时标识、版本日期和提交哈希。自包含可执行文件不要求目标系统预先安装 .NET 运行时。

## CLI 使用

以下示例中的 `OplusDigestUtils.App.exe` 表示下载后重命名的文件或本地构建产物。

```text
OplusDigestUtils.App.exe <Digest 文件路径> [更多文件路径] [--json]
```

解析单个文件：

```powershell
.\OplusDigestUtils.App.exe ".\digest.mbn"
```

解析多个文件并输出 JSON：

```powershell
.\OplusDigestUtils.App.exe ".\digest.mbn" ".\digest.elf" --json
```

直接从源码运行：

```powershell
dotnet run --project OplusDigestUtils.App/OplusDigestUtils.App.csproj -c Release -- ".\digest.mbn" --json
```

### 参数

| 参数 | 说明 |
| --- | --- |
| `<Digest 文件路径> [更多文件路径]` | 一个或多个 Digest 文件；路径包含空格时需要加引号 |
| `--json` | 输出缩进后的 JSON；单个结果为对象，多个结果为数组 |
| `-h`、`--help` | 显示用法 |

未指定 `--json` 时，CLI 会输出解析状态、OEM、SoC、验证状态、Root CA 哈希、Digest 头和每条分区记录。多文件模式会保留每个输入的独立结果；常规的结构解析失败不会阻止后续文件继续解析。

### 退出码

| 退出码 | 含义 |
| --- | --- |
| `0` | 所有文件均解析成功，或仅请求帮助 |
| `1` | 至少一个文件解析失败 |
| `2` | 未提供文件或传入未知的 `--` 选项 |

以上是程序正常控制流程中的退出码；未处理的文件系统异常可能由运行时返回其他退出码。

## 类库使用

`OplusDigestParser` 实现了 `IOplusDigestParser`。文件路径重载适合直接读取 Digest 文件：

```csharp
using OplusDigestUtils;
using OplusDigestUtils.Models;

IOplusDigestParser parser = new OplusDigestParser();

if (!parser.TryParse("digest.mbn", out OplusDigestParseResult result))
{
    Console.Error.WriteLine(result.ErrorMessage);
    return;
}

Console.WriteLine($"OEM: {result.OemType}");
Console.WriteLine($"SoC: {result.SocType}");
Console.WriteLine($"Root CA: {result.RootCaHashHex}");
Console.WriteLine($"Hash table: {result.VerificationStatus}");

foreach (OplusDigestPartition partition in result.Digest.Partitions)
{
    Console.WriteLine(
        $"{partition.Label}: {partition.FileName}, " +
        $"sector={partition.StartSector}, count={partition.Sectors}, " +
        $"hash={partition.HashHex}");
}
```

已有内存数据时，可以使用 Span 重载：

```csharp
byte[] data = File.ReadAllBytes("digest.elf");

if (!parser.TryParse(data, out OplusDigestParseResult result))
    Console.Error.WriteLine(result.ErrorMessage);
```

> `TryParse` 返回 `true`、或 `IsSuccess` 为 `true`，只表示 Qualcomm 外层镜像及 Oplus Digest 结构解析成功。是否通过封装镜像哈希表检查，必须另外判断 `VerificationStatus`；即使该字段为 `Invalid`，结构解析仍可能成功。

### 主要结果字段

| 字段 | 含义 |
| --- | --- |
| `IsSuccess` | Oplus Digest 结构是否解析成功 |
| `Digest.Header` | Digest 头，包括 Magic、版本、头长度、分区数量和分区记录长度 |
| `Digest.Partitions` | 分区记录集合 |
| `OemType` | 从 Qualcomm 外层镜像识别出的 OEM 类型 |
| `SocType` | 从 Qualcomm 外层镜像识别出的 SoC 类型 |
| `RootCaHashHex` | Root CA 哈希的十六进制字符串 |
| `VerificationStatus` | Qualcomm 外层镜像的哈希表验证状态 |
| `ErrorMessage` | 结构解析失败时的错误信息；成功时为 `null` |

每个 `OplusDigestPartition` 包含以下字段：

| 字段 | 含义 |
| --- | --- |
| `Label` | 分区标签 |
| `FileName` | Digest 中记录的分区文件名 |
| `AllowRead` | 是否允许读取 |
| `AllowWrite` | 是否允许写入 |
| `StartSector` | 起始扇区 |
| `Sectors` | 扇区数量 |
| `HashHex` | Digest 中记录的分区哈希十六进制字符串 |

`VerificationStatus` 使用 `QcomVerificationStatus`，常见状态包括 `NotChecked`、`NotPresent`、`Valid`、`Invalid` 和 `Unsupported`。

## 从源码构建

需要 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)。在仓库根目录执行：

```powershell
dotnet restore OplusDigestUtils.slnx
dotnet build OplusDigestUtils.slnx -c Release --no-restore
```

发布 Windows x64 Native AOT CLI：

```powershell
dotnet restore OplusDigestUtils.App/OplusDigestUtils.App.csproj -r win-x64
dotnet publish OplusDigestUtils.App/OplusDigestUtils.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  --no-restore `
  -o artifacts/win-x64
```

可将 RID 替换为 `win-x86` 或 `win-arm64`。本地 Native AOT 发布还需要 Windows C/C++ 原生编译工具链。

## 许可证

本项目基于 [MIT License](LICENSE) 开源。
