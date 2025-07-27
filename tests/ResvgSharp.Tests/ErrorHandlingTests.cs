using System;
using Xunit;
using ResvgSharp;
using ResvgSharp.Exceptions;

namespace ResvgSharp.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public void RenderToPng_InvalidSvg_ThrowsParseException()
    {
        var invalidSvg = "<svg><invalid></svg>";
        
        Assert.Throws<ResvgParseException>(() => Resvg.RenderToPng(invalidSvg));
    }

    [Fact]
    public void RenderToPng_MalformedXml_ThrowsParseException()
    {
        var malformedSvg = "<svg width='100' height='100'><rect></svg>";
        
        Assert.Throws<ResvgParseException>(() => Resvg.RenderToPng(malformedSvg));
    }

    [Fact]
    public void RenderToPng_EmptySvg_ThrowsParseException()
    {
        var emptySvg = "<svg></svg>";
        
        Assert.Throws<ResvgParseException>(() => Resvg.RenderToPng(emptySvg));
    }

    [Fact]
    public void RenderToPng_VeryLargeDimensions_HandlesGracefully()
    {
        var svg = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
            <rect width=""100"" height=""100"" fill=""blue""/>
        </svg>";
        
        var options = new ResvgOptions
        {
            Width = 20000,
            Height = 20000
        };
        
        var pngBytes = Resvg.RenderToPng(svg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
    }

    [Fact]
    public void RenderToPng_InvalidDpi_UsesDefault()
    {
        var svg = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
            <rect width=""100"" height=""100"" fill=""green""/>
        </svg>";
        
        var options = new ResvgOptions
        {
            Dpi = -1
        };
        
        var pngBytes = Resvg.RenderToPng(svg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
    }

    [Fact]
    public void RenderToPng_InvalidBackgroundColor_IgnoresBackground()
    {
        var svg = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
            <rect width=""50"" height=""50"" fill=""red""/>
        </svg>";
        
        var options = new ResvgOptions
        {
            Background = "invalid-color-value"
        };
        
        var pngBytes = Resvg.RenderToPng(svg, options);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
    }

    [Theory]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><text>Test</text></svg>")]
    [InlineData("<svg viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'><circle cx='50' cy='50' r='40'/></svg>")]
    public void RenderToPng_EdgeCaseSvgs_HandlesCorrectly(string svg)
    {
        var pngBytes = Resvg.RenderToPng(svg);
        
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);
    }
}