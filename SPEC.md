# ResvgSharp Specification

## Overview

**ResvgSharp** is a cross-platform .NET wrapper for the high-performance [resvg](https://github.com/RazrFalcon/resvg) SVG rendering library written in Rust. It enables rendering SVG strings to PNG images with full support for custom fonts provided as in-memory `byte[]`.

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
- ✅ No system font dependency
- ✅ Optimized for use in Azure Functions, containers, and desktop apps

## Public API

```csharp
public static class Resvg
{
    /// <summary>
    /// Renders an SVG string to a PNG image using the given fonts.
    /// </summary>
    /// <param name="svg">SVG markup as string</param>
    /// <param name="fonts">Font files as byte arrays (TTF/OTF)</param>
    /// <returns>PNG image as byte array</returns>
    public static byte[] RenderToPng(string svg, byte[][] fonts);
}
```

## Native FFI Signature (Rust)

```
#[no_mangle]
pub extern "C" fn render_svg_to_png_bytes(
    svg_data: *const c_char,
    fonts: *const *const u8,
    font_lens: *const usize,
    font_count: usize,
    out_buf: *mut *mut u8,
    out_len: *mut usize,
) -> i32;
````


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

* Receives the SVG as a char*

* Receives an array of font blobs (uint8_t*[])

* Writes to the provided output path

* Fonts are loaded using fontdb::Database::load_font_source


## Folder Layout

```
ResvgSharp/
├── lib/
│   └── netstandard2.0/ResvgSharp.dll
├── runtimes/
│   ├── win-x64/native/resvg_wrapper.dll
│   ├── linux-x64/native/libresvg_wrapper.so
│   └── osx-x64/native/libresvg_wrapper.dylib
├── src/
│   ├── ResvgSharp.csproj
│   └── Resvg.cs
├── rust/
│   ├── Cargo.toml
│   └── src/lib.rs
└── .github/workflows/build.yml
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
    var fonts = new[] { File.ReadAllBytes("fonts/Inter-Regular.ttf") };
    
    var pngBytes = Resvg.RenderToPng(svg, fonts);
    
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

