# ResvgSharp Specification

## Overview

**ResvgSharp** is a cross-platform .NET wrapper for the high-performance [resvg](https://github.com/linebender/resvg) SVG rendering library written in Rust. It enables rendering SVG strings to PNG images with full support for custom fonts.

## Requirements

- .NET 9.0 or higher
- Supported platforms: Windows (x64), Linux (x64), macOS (x64/arm64)
- Supported font formats: TrueType (.ttf), OpenType (.otf)

---

## Features

- ✅ Render SVG strings to PNG
- ✅ Use custom fonts (in-memory `byte[]`)
- ✅ Cross-platform: Windows, Linux, macOS
- ✅ Packaged as a single NuGet package
- ✅ System font control (can disable or supplement)
- ✅ Optimized for use in Azure Functions, containers, and desktop apps

## Public API

```csharp
public static class Resvg
{
    /// <summary>
    /// Renders an SVG string to a PNG image using the specified options.
    /// </summary>
    /// <param name="svg">SVG markup as string</param>
    /// <param name="options">Rendering options (optional)</param>
    /// <returns>PNG image as byte array</returns>
    public static byte[] RenderToPng(string svg, ResvgOptions? options = null);
}

public class ResvgOptions
{
    /// <summary>
    /// Sets the output width in pixels. Aspect ratio is preserved if Height is not set.
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// Sets the output height in pixels. Aspect ratio is preserved if Width is not set.
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// Zooms the image by a factor. Overrides Width/Height if set.
    /// </summary>
    public float? Zoom { get; set; }
    
    /// <summary>
    /// Sets the resolution in DPI. Range: 10-4000.
    /// </summary>
    public int Dpi { get; set; } = 96;
    
    /// <summary>
    /// Font files as byte arrays (TTF/OTF) to use for rendering.
    /// </summary>
    public byte[][]? UseFonts { get; set; }
    
    /// <summary>
    /// Path to a font file to use for rendering.
    /// </summary>
    public string? UseFontFile { get; set; }
    
    /// <summary>
    /// Directory containing font files to use for rendering.
    /// </summary>
    public string? UseFontDir { get; set; }
    
    /// <summary>
    /// If true, system fonts will not be loaded.
    /// </summary>
    public bool SkipSystemFonts { get; set; } = false;
    
    /// <summary>
    /// Sets the background color. Examples: "red", "#fff", "#fff000"
    /// </summary>
    public string? Background { get; set; }
    
    /// <summary>
    /// Renders only the element with the specified ID.
    /// </summary>
    public string? ExportId { get; set; }
    
    /// <summary>
    /// Use image size instead of object size when exporting by ID.
    /// </summary>
    public bool ExportAreaPage { get; set; } = false;
    
    /// <summary>
    /// Use drawing's tight bounding box instead of image size.
    /// </summary>
    public bool ExportAreaDrawing { get; set; } = true;
    
    /// <summary>
    /// Directory used for resolving relative paths in the SVG.
    /// </summary>
    public string? ResourcesDir { get; set; }
}
```

## Native FFI Signature (Rust)

```rust
#[repr(C)]
pub struct RenderOptions {
    pub width: i32,          // -1 for unset
    pub height: i32,         // -1 for unset
    pub zoom: f32,           // 0.0 for unset
    pub dpi: i32,
    pub skip_system_fonts: bool,
    pub background: *const c_char,     // null for unset
    pub export_id: *const c_char,      // null for unset
    pub export_area_page: bool,
    pub export_area_drawing: bool,
    pub resources_dir: *const c_char,  // null for unset
    pub fonts: *const *const u8,       // array of font data pointers
    pub font_lens: *const usize,       // array of font data lengths
    pub font_count: usize,             // number of fonts
    pub font_file: *const c_char,      // null for unset
    pub font_dir: *const c_char,       // null for unset
}

#[no_mangle]
pub extern "C" fn render_svg_to_png_with_options(
    svg_data: *const c_char,
    options: *const RenderOptions,
    out_buf: *mut *mut u8,
    out_len: *mut usize,
) -> i32;
```


## Build System

### 📦 GitHub Actions

CI builds and publishes the package for all platforms:

* 🧱 Builds Rust cdylib with cross-compilation targets

* 🔀 Packages runtime binaries under runtimes/<rid>/native

* 📦 Packs NuGet using .nuspec or .csproj with <Content> and <None> includes

* 🚀 Deploys to NuGet.org on push to main with version tag. Repo has NUGET_API_TOKEN


## Internal Architecture

* C# calls into Rust via DllImport.

* Rust exposes an extern "C" FFI that:
  - Receives the SVG as a char*
  - Receives a RenderOptions struct with all rendering parameters
  - Returns PNG data via output buffer pointer
  - Manages memory allocation for the output buffer

* Font loading priority:
  1. In-memory fonts (UseFonts)
  2. Font file path (UseFontFile)
  3. Font directory (UseFontDir)
  4. System fonts (unless SkipSystemFonts is true)

* Fonts are loaded using fontdb::Database::load_font_source

## Folder Layout

```
ResvgSharp/
├── src/
│   └── ResvgSharp/
│       ├── ResvgSharp.csproj
│       ├── Resvg.cs
│       └── Exceptions/
│           ├── ResvgException.cs
│           ├── ResvgParseException.cs
│           ├── ResvgPngRenderException.cs
│           └── ResvgFontLoadException.cs
├── tests/
│   └── ResvgSharp.Tests/
│       ├── ResvgSharp.Tests.csproj
│       ├── RenderTests.cs
│       ├── FontTests.cs
│       ├── ErrorHandlingTests.cs
│       └── TestAssets/
│           ├── fonts/
│           └── svg/
├── native/
│   └── resvg-wrapper/
│       ├── Cargo.toml
│       └── src/
│           └── lib.rs
├── build/
│   └── runtimes/
│       ├── win-x64/native/resvg_wrapper.dll
│       ├── linux-x64/native/libresvg_wrapper.so
│       ├── linux-arm64/native/libresvg_wrapper.so
│       ├── osx-x64/native/libresvg_wrapper.dylib
│       └── osx-arm64/native/libresvg_wrapper.dylib
├── ResvgSharp.sln
├── Directory.Build.props
├── .github/
│   └── workflows/
│       └── build.yml
└── nuget/
    └── ResvgSharp.nuspec
```

## Error Handling

| Rust Return Code | Meaning | C# Exception |
|------------------|---------|---------------|
| 0 | Success | - |
| 1 | SVG parse error | `ResvgParseException` |
| 2 | PNG write failure | `ResvgPngRenderException` |
| 3 | Font load failure | `ResvgFontLoadException` |
| 4 | Memory error | `OutOfMemoryException` |

### Exception Types

```csharp
public class ResvgException : Exception { }
public class ResvgParseException : ResvgException { }
public class ResvgPngRenderException : ResvgException { }
public class ResvgFontLoadException : ResvgException { }
```

## Testing

### Goals

Ensure `ResvgSharp`:

- Correctly renders valid SVG input
- Accurately loads and applies custom fonts from `byte[]`
- Produces consistent output across all supported platforms (Windows, Linux, macOS)
- Handles invalid input gracefully
- Works in .NET environments including Azure Functions, console apps, and containers

---

### Unit Tests

Framework: `xUnit`

```csharp
[Theory]
[InlineData("sample.svg", "expected.png")]
public void RenderSvgToPng_BasicSvg_RendersCorrectly(string svgFile, string expectedPng)
{
    var svg = File.ReadAllText(svgFile);
    var options = new ResvgOptions
    {
        UseFonts = new[] { File.ReadAllBytes("fonts/Inter-Regular.ttf") },
        Width = 800,
        Dpi = 96
    };
    
    var pngBytes = Resvg.RenderToPng(svg, options);
    
    // Verify PNG output
    Assert.NotNull(pngBytes);
    Assert.True(pngBytes.Length > 0);
    // Compare with expected output or verify PNG header
    Assert.Equal(0x89, pngBytes[0]); // PNG signature
}
```

| Test Name                        | Description                                                         |
| -------------------------------- | ------------------------------------------------------------------- |
| `Render_Minimal_SVG`             | Renders a minimal valid SVG                                         |
| `Render_With_Inter_Font`         | Renders SVG using a custom font in-memory                           |
| `Render_Invalid_SVG_Throws`      | Ensures malformed SVG throws appropriate exception                  |
| `Render_Missing_Font_Fallbacks`  | Renders with no fonts and falls back to default behavior            |
| `Render_With_Multiple_Fonts`     | Renders using two or more fonts in the font array                   |
| `Render_Large_SVG_Performance`   | Renders a high-complexity SVG within reasonable time/memory limits  |
| `Render_Empty_Font_Bytes_Throws` | Fails gracefully when font byte\[] is empty                         |
| `Render_Nonexistent_Output_Path` | Ensures a proper exception when output path is invalid or read-only |

## Performance Guidelines

### Memory Usage

- SVG parsing: ~2-3x the SVG file size
- Font loading: ~1.5x per font file
- Rendering buffer: Width × Height × 4 bytes (RGBA)
- Peak memory: SVG + Fonts + Render Buffer + ~10MB overhead

### Limits

- Maximum SVG size: 50MB
- Maximum output dimensions: 16384×16384 pixels
- Maximum fonts: 100 fonts per render
- Rendering timeout: 30 seconds

