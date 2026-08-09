# OplusDigestUtils

[![CI and Publish Native AOT](https://github.com/JacksonOnly/OplusDigestUtils/actions/workflows/publish-aot.yml/badge.svg)](https://github.com/JacksonOnly/OplusDigestUtils/actions/workflows/publish-aot.yml)
[![NuGet version (OplusDigestUtils)](https://img.shields.io/nuget/v/OplusDigestUtils.svg?style=flat-square)](https://www.nuget.org/packages/OplusDigestUtils/)

OplusDigestUtils 用于解析和验证 Oppo Realme Oneplus 固件包内的 Digest mbn或elf，不过只支持的是它的自定义Digest，
对于高通官方规定的Digest 也就是仅含有哈希表的格式是不支持的。

仓库包含两个可直接使用的项目：

- `OplusDigestUtils`：目标框架为 `netstandard2.0` 和 `net10.0` 的类库。
- `OplusDigestUtils.App`：目标框架为 `net10.0-windows` 的命令行工具，支持 Native AOT 发布。
