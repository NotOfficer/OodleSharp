using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using OodleDotNet;

namespace OodleSharp.Tests;

file static class Helper
{
    private static readonly HttpClient _client = new(new SocketsHttpHandler
    {
        UseProxy = false,
        UseCookies = true,
        AutomaticDecompression = DecompressionMethods.All
    });

    public static async Task<string> DownloadOodleAsync()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("this test is not supported on the current operating system");
        }

        const string baseUrl = "https://github.com/WorkingRobot/OodleUE/releases/download/2026-06-04-1357/"; // 2.9.16

        string archPart = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("this test is not supported on the current process architecture")
        };

        string url;
        string entryName;

        if (OperatingSystem.IsWindows())
        {
            url = baseUrl + $"msvc-{archPart}-release.zip";
            entryName = "bin/oodle-data-shared.dll";
        }
        else if (OperatingSystem.IsLinux())
        {
            url = baseUrl + $"gcc-{archPart}-release.zip";
            entryName = "lib/liboodle-data-shared.so";
        }
        else
        {
            throw new UnreachableException();
        }

        using HttpResponseMessage response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using Stream responseStream = await response.Content.ReadAsStreamAsync();
        await using var zip = new ZipArchive(responseStream, ZipArchiveMode.Read);

        ZipArchiveEntry? entry = zip.GetEntry(entryName);
        ArgumentNullException.ThrowIfNull(entry, "oodle entry in zip not found");

        await using Stream entryStream = await entry.OpenAsync();
        string filePath = Path.GetTempFileName();

        await using FileStream fs = File.Create(filePath);
        await entryStream.CopyToAsync(fs);

        return filePath;
    }

    public static string GetRandomString(int length)
    {
        const string pool = "0123456789ABCDEF";
        return RandomNumberGenerator.GetString(pool, length);
    }
}

public class Tests : IAsyncLifetime
{
    private string _filePath = null!;
    private Oodle _oodle = null!;

    public async ValueTask InitializeAsync()
    {
        _filePath = await Helper.DownloadOodleAsync();
        _oodle = new Oodle(_filePath);
    }

    [Fact]
    public void CompressAndDecompress()
    {
        string randomString = Helper.GetRandomString(8191);
        byte[] randomStringBuffer = Encoding.ASCII.GetBytes(randomString);
        byte[] decompressedBuffer = GC.AllocateUninitializedArray<byte>(randomStringBuffer.Length);

        List<OodleCompressor> compressors =
        [
            OodleCompressor.Kraken,
            OodleCompressor.Leviathan,
            OodleCompressor.Mermaid,
            OodleCompressor.Selkie,
            OodleCompressor.Hydra
        ];

        List<OodleCompressionLevel> levels =
        [
            OodleCompressionLevel.HyperFast4,
            OodleCompressionLevel.HyperFast3,
            OodleCompressionLevel.HyperFast2,
            OodleCompressionLevel.HyperFast1,
            OodleCompressionLevel.SuperFast,
            OodleCompressionLevel.VeryFast,
            OodleCompressionLevel.Fast,
            OodleCompressionLevel.Normal,
            OodleCompressionLevel.Optimal1,
            OodleCompressionLevel.Optimal2,
            OodleCompressionLevel.Optimal3,
            OodleCompressionLevel.Optimal4,
            OodleCompressionLevel.Optimal5
        ];

        int compressedBufferSize = (int)compressors.Max(
            x => _oodle.GetCompressedBufferSizeNeeded(x, randomString.Length));
        Assert.NotEqual(0, compressedBufferSize);
        byte[] compressedBuffer = GC.AllocateUninitializedArray<byte>(compressedBufferSize);

        foreach (OodleCompressor compressor in compressors)
        {
            foreach (OodleCompressionLevel compressionLevel in levels)
            {
                compressedBuffer.AsSpan().Clear();
                int compressedSize = (int)_oodle.Compress(compressor, compressionLevel, randomStringBuffer, compressedBuffer);
                Assert.True(compressedSize > 0);

                decompressedBuffer.AsSpan().Clear();
                int decompressedSize = (int)_oodle.Decompress(compressedBuffer.AsSpan(0, compressedSize), decompressedBuffer);
                Assert.Equal(decompressedSize, randomStringBuffer.Length);
                Assert.Equal(randomStringBuffer, decompressedBuffer);

                decompressedBuffer.AsSpan().Clear();
                decompressedSize = OodleDecompressor.Decompress(compressedBuffer.AsSpan(0, compressedSize), decompressedBuffer);
                Assert.Equal(decompressedSize, randomStringBuffer.Length);
                Assert.Equal(randomStringBuffer, decompressedBuffer);
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _oodle.Dispose();
        File.Delete(_filePath);
        return ValueTask.CompletedTask;
    }
}
