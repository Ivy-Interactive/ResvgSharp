using System;
using System.IO;
using Xunit;
using ResvgSharp;

namespace ResvgSharp.Tests;

public class RenderTests
{
    private const string TestSvg = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
        <rect width=""100"" height=""100"" fill=""red""/>
    </svg>";

    private static readonly string OutputDir = Path.Combine("TestOutput");

    static RenderTests()
    {
        Directory.CreateDirectory(OutputDir);
    }

    private void SavePngOutput(byte[] pngBytes, string filename)
    {
        var outputPath = Path.Combine(OutputDir, filename);
        File.WriteAllBytes(outputPath, pngBytes);
    }

    [Fact]
    public void RenderToPng_MinimalSvg_RendersCorrectly()
    {
        var pngBytes = Resvg.RenderToPng(TestSvg);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        Assert.Equal(0x89, pngBytes[0]);
        Assert.Equal(0x50, pngBytes[1]);
        Assert.Equal(0x4E, pngBytes[2]);
        Assert.Equal(0x47, pngBytes[3]);
        
        SavePngOutput(pngBytes, "minimal-svg.png");
    }

    [Fact]
    public void RenderToPng_WithCustomDimensions_RendersCorrectSize()
    {
        var options = new ResvgOptions
        {
            Width = 200,
            Height = 200
        };
        
        var pngBytes = Resvg.RenderToPng(TestSvg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        
        SavePngOutput(pngBytes, "custom-dimensions-200x200.png");
    }

    [Fact]
    public void RenderToPng_WithZoom_RendersScaled()
    {
        var options = new ResvgOptions
        {
            Zoom = 2.0f
        };
        
        var pngBytes = Resvg.RenderToPng(TestSvg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        
        SavePngOutput(pngBytes, "zoom-2x.png");
    }

    [Fact]
    public void RenderToPng_WithBackgroundColor_RendersWithBackground()
    {
        var options = new ResvgOptions
        {
            Background = "white"
        };
        
        var pngBytes = Resvg.RenderToPng(TestSvg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        
        SavePngOutput(pngBytes, "white-background.png");
    }

    [Fact]
    public void RenderToPng_WithDifferentDpi_RendersCorrectly()
    {
        var options = new ResvgOptions
        {
            Dpi = 150
        };
        
        var pngBytes = Resvg.RenderToPng(TestSvg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        
        SavePngOutput(pngBytes, "dpi-150.png");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    public void RenderToPng_NullOrEmptySvg_ThrowsArgumentNullException(string? svg)
    {
        Assert.Throws<ArgumentNullException>(() => Resvg.RenderToPng(svg!));
    }

    [Fact]
    public void RenderToPng_ComplexSvg_RendersSuccessfully()
    {
        var complexSvg = @"<svg width=""300"" height=""200"" xmlns=""http://www.w3.org/2000/svg"">
            <defs>
                <linearGradient id=""grad1"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
                    <stop offset=""0%"" style=""stop-color:rgb(255,255,0);stop-opacity:1"" />
                    <stop offset=""100%"" style=""stop-color:rgb(255,0,0);stop-opacity:1"" />
                </linearGradient>
            </defs>
            <rect width=""300"" height=""200"" fill=""url(#grad1)"" />
            <circle cx=""150"" cy=""100"" r=""50"" fill=""blue"" opacity=""0.5"" />
            <text x=""150"" y=""100"" text-anchor=""middle"" fill=""white"" font-size=""20"" font-family=""Arial"">Test</text>
        </svg>";
        
        var pngBytes = Resvg.RenderToPng(complexSvg);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
        
        SavePngOutput(pngBytes, "complex-gradient-circle-text.png");
    }
}