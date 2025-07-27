using System;
using System.IO;
using Xunit;
using ResvgSharp;

namespace ResvgSharp.Tests;

public class VisualTests
{
    private static readonly string OutputDir = Path.Combine("TestOutput");

    static VisualTests()
    {
        Directory.CreateDirectory(OutputDir);
    }

    private void SavePngOutput(byte[] pngBytes, string filename)
    {
        var outputPath = Path.Combine(OutputDir, filename);
        File.WriteAllBytes(outputPath, pngBytes);
    }

    [Fact]
    public void RenderToPng_HexBackgroundColors_RendersCorrectly()
    {
        var svg = @"<svg width=""150"" height=""150"" xmlns=""http://www.w3.org/2000/svg"">
            <circle cx=""75"" cy=""75"" r=""60"" fill=""#ff6b35"" stroke=""#004643"" stroke-width=""4""/>
            <text x=""75"" y=""80"" text-anchor=""middle"" fill=""white"" font-size=""16"" font-weight=""bold"">Orange</text>
        </svg>";

        var options = new ResvgOptions
        {
            Background = "#e9f5db"
        };

        var pngBytes = Resvg.RenderToPng(svg, options);

        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);

        SavePngOutput(pngBytes, "hex-background-colors.png");
    }

    [Fact]
    public void RenderToPng_ComplexShapes_RendersCorrectly()
    {
        var svg = @"<svg width=""300"" height=""300"" xmlns=""http://www.w3.org/2000/svg"">
            <defs>
                <radialGradient id=""radial"" cx=""50%"" cy=""50%"" r=""50%"">
                    <stop offset=""0%"" stop-color=""#ffd700""/>
                    <stop offset=""100%"" stop-color=""#ff8c00""/>
                </radialGradient>
                <pattern id=""dots"" patternUnits=""userSpaceOnUse"" width=""20"" height=""20"">
                    <circle cx=""10"" cy=""10"" r=""3"" fill=""#333""/>
                </pattern>
            </defs>
            
            <!-- Background -->
            <rect width=""300"" height=""300"" fill=""url(#radial)""/>
            
            <!-- Star shape -->
            <path d=""M150,50 L165,90 L210,90 L175,115 L190,160 L150,135 L110,160 L125,115 L90,90 L135,90 Z"" 
                  fill=""url(#dots)"" stroke=""#8b0000"" stroke-width=""3""/>
            
            <!-- Text with shadow effect -->
            <text x=""150"" y=""250"" text-anchor=""middle"" font-size=""24"" font-weight=""bold"" 
                  fill=""#ffffff"" stroke=""#000000"" stroke-width=""1"">ResvgSharp</text>
        </svg>";

        var pngBytes = Resvg.RenderToPng(svg);

        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);

        SavePngOutput(pngBytes, "complex-shapes-gradients.png");
    }

    [Fact]
    public void RenderToPng_HighResolution_RendersCorrectly()
    {
        var svg = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
            <rect width=""100"" height=""100"" fill=""#2c3e50""/>
            <circle cx=""50"" cy=""50"" r=""30"" fill=""#e74c3c""/>
            <rect x=""40"" y=""40"" width=""20"" height=""20"" fill=""#f39c12"" rx=""5""/>
        </svg>";

        var options = new ResvgOptions
        {
            Width = 800,
            Height = 800,
            Dpi = 300
        };

        var pngBytes = Resvg.RenderToPng(svg, options);

        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);

        SavePngOutput(pngBytes, "high-resolution-800x800.png");
    }

    [Fact]
    public void RenderToPng_TextWithDifferentFonts_RendersCorrectly()
    {
        var svg = @"<svg width=""400"" height=""200"" xmlns=""http://www.w3.org/2000/svg"">
            <rect width=""400"" height=""200"" fill=""#f8f9fa""/>
            
            <text x=""200"" y=""40"" text-anchor=""middle"" font-size=""18"" font-weight=""bold"" fill=""#212529"">
                Font Rendering Test
            </text>
            
            <text x=""20"" y=""80"" font-size=""16"" fill=""#495057"">
                Regular text with system fonts
            </text>
            
            <text x=""20"" y=""110"" font-size=""14"" font-style=""italic"" fill=""#6c757d"">
                Italic text example
            </text>
            
            <text x=""20"" y=""140"" font-size=""12"" font-weight=""bold"" fill=""#343a40"">
                Bold text in smaller size
            </text>
            
            <text x=""20"" y=""170"" font-size=""20"" fill=""#dc3545"" font-family=""monospace"">
                Monospace: console.log('Hello');
            </text>
        </svg>";

        var pngBytes = Resvg.RenderToPng(svg);

        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);

        SavePngOutput(pngBytes, "text-different-fonts.png");
    }

    [Fact]
    public void RenderToPng_TransparencyAndOpacity_RendersCorrectly()
    {
        var svg = @"<svg width=""200"" height=""200"" xmlns=""http://www.w3.org/2000/svg"">
            <!-- Checkerboard pattern to show transparency -->
            <defs>
                <pattern id=""checkerboard"" patternUnits=""userSpaceOnUse"" width=""20"" height=""20"">
                    <rect width=""10"" height=""10"" fill=""#ddd""/>
                    <rect x=""10"" y=""10"" width=""10"" height=""10"" fill=""#ddd""/>
                    <rect x=""10"" y=""0"" width=""10"" height=""10"" fill=""#aaa""/>
                    <rect x=""0"" y=""10"" width=""10"" height=""10"" fill=""#aaa""/>
                </pattern>
            </defs>
            
            <rect width=""200"" height=""200"" fill=""url(#checkerboard)""/>
            
            <!-- Overlapping shapes with different opacities -->
            <circle cx=""70"" cy=""70"" r=""40"" fill=""red"" opacity=""0.7""/>
            <circle cx=""100"" cy=""70"" r=""40"" fill=""green"" opacity=""0.7""/>
            <circle cx=""85"" cy=""100"" r=""40"" fill=""blue"" opacity=""0.7""/>
        </svg>";

        var pngBytes = Resvg.RenderToPng(svg);

        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);

        SavePngOutput(pngBytes, "transparency-opacity.png");
    }
}