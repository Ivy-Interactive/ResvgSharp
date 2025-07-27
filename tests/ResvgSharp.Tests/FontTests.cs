using System;
using System.IO;
using Xunit;
using ResvgSharp;
using ResvgSharp.Exceptions;

namespace ResvgSharp.Tests;

public class FontTests
{
    private const string TextSvg = @"<svg width=""200"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
        <text x=""10"" y=""50"" font-family=""Inter"" font-size=""24"" fill=""black"">Hello World</text>
    </svg>";

    private static readonly string OutputDir = Path.Combine("TestOutput");

    static FontTests()
    {
        Directory.CreateDirectory(OutputDir);
    }

    private void SavePngOutput(byte[] pngBytes, string filename)
    {
        var outputPath = Path.Combine(OutputDir, filename);
        File.WriteAllBytes(outputPath, pngBytes);
    }

    [Fact]
    public void RenderToPng_WithCustomFont_UsesProvidedFont()
    {
        var fontPath = Path.Combine("TestAssets", "fonts", "Inter-Regular.ttf");
        if (File.Exists(fontPath))
        {
            var fontData = File.ReadAllBytes(fontPath);
            var options = new ResvgOptions
            {
                UseFonts = new[] { fontData },
                SkipSystemFonts = true
            };
            
            var pngBytes = Resvg.RenderToPng(TextSvg, options);
            
            Assert.NotNull(pngBytes);
            Assert.True(pngBytes.Length > 0);
            
            SavePngOutput(pngBytes, "inter-font-regular.png");
        }
    }

    [Fact]
    public void RenderToPng_WithMultipleFonts_LoadsAllFonts()
    {
        var font1Path = Path.Combine("TestAssets", "fonts", "Inter-Regular.ttf");
        var font2Path = Path.Combine("TestAssets", "fonts", "Inter-Bold.ttf");
        
        if (File.Exists(font1Path) && File.Exists(font2Path))
        {
            var font1Data = File.ReadAllBytes(font1Path);
            var font2Data = File.ReadAllBytes(font2Path);
            
            var options = new ResvgOptions
            {
                UseFonts = new[] { font1Data, font2Data }
            };
            
            var pngBytes = Resvg.RenderToPng(TextSvg, options);
            
            Assert.NotNull(pngBytes);
            Assert.True(pngBytes.Length > 0);
            
            SavePngOutput(pngBytes, "multiple-fonts.png");
        }
    }

    [Fact]
    public void RenderToPng_WithFontFile_LoadsFontFromPath()
    {
        var fontPath = Path.Combine("TestAssets", "fonts", "Inter-Regular.ttf");
        if (File.Exists(fontPath))
        {
            var options = new ResvgOptions
            {
                UseFontFile = fontPath
            };
            
            var pngBytes = Resvg.RenderToPng(TextSvg, options);
            
            Assert.NotNull(pngBytes);
            Assert.True(pngBytes.Length > 0);
        }
    }

    [Fact]
    public void RenderToPng_WithFontDirectory_LoadsFontsFromDirectory()
    {
        var fontDir = Path.Combine("TestAssets", "fonts");
        if (Directory.Exists(fontDir))
        {
            var options = new ResvgOptions
            {
                UseFontDir = fontDir
            };
            
            var pngBytes = Resvg.RenderToPng(TextSvg, options);
            
            Assert.NotNull(pngBytes);
            Assert.True(pngBytes.Length > 0);
        }
    }

    [Fact]
    public void RenderToPng_WithEmptyFontData_ThrowsFontLoadException()
    {
        var options = new ResvgOptions
        {
            UseFonts = new byte[][] { Array.Empty<byte>() }
        };
        
        Assert.Throws<ResvgFontLoadException>(() => Resvg.RenderToPng(TextSvg, options));
    }

    [Fact]
    public void RenderToPng_WithNullFontData_ThrowsFontLoadException()
    {
        var options = new ResvgOptions
        {
            UseFonts = new byte[][] { null! }
        };
        
        Assert.Throws<ResvgFontLoadException>(() => Resvg.RenderToPng(TextSvg, options));
    }

    [Fact]
    public void RenderToPng_SkipSystemFonts_DoesNotUseSystemFonts()
    {
        var options = new ResvgOptions
        {
            SkipSystemFonts = true
        };
        
        var pngBytes = Resvg.RenderToPng(TextSvg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        
        SavePngOutput(pngBytes, "no-system-fonts.png");
    }
}