**ResvgSharp** is a cross-platform .NET wrapper for the high-performance [resvg](https://github.com/linebender/resvg) SVG rendering library written in Rust. It enables rendering SVG strings to PNG images with full support for custom fonts.

## Usage

### Basic Usage

The simplest way to render an SVG to PNG:

```csharp
using ResvgSharp;

string svg = @"<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
    <rect width=""100"" height=""100"" fill=""red""/>
</svg>";

byte[] pngBytes = Resvg.RenderToPng(svg);
File.WriteAllBytes("output.png", pngBytes);
```

### Advanced Usage with Options

You can customize the rendering with `ResvgOptions`:

```csharp
using ResvgSharp;

var options = new ResvgOptions
{
    Width = 200,           // Custom width
    Height = 200,          // Custom height
    Zoom = 2.0f,           // Scale factor
    Dpi = 150,             // DPI (default: 96)
    Background = "white"   // Background color
};

byte[] pngBytes = Resvg.RenderToPng(svg, options);
```

### Using Custom Fonts

ResvgSharp supports loading custom fonts in three ways:

#### 1. In-Memory Font Data
```csharp
byte[] fontData = File.ReadAllBytes("CustomFont.ttf");

var options = new ResvgOptions
{
    UseFonts = new[] { fontData },
    SkipSystemFonts = true  // Optional: skip system fonts
};

byte[] pngBytes = Resvg.RenderToPng(svg, options);
```

#### 2. Font File Path
```csharp
var options = new ResvgOptions
{
    UseFontFile = "path/to/font.ttf"
};

byte[] pngBytes = Resvg.RenderToPng(svg, options);
```

#### 3. Font Directory
```csharp
var options = new ResvgOptions
{
    UseFontDir = "path/to/fonts/directory"
};

byte[] pngBytes = Resvg.RenderToPng(svg, options);
```

### Export Options

Control what gets exported:

```csharp
var options = new ResvgOptions
{
    ExportId = "specific-element-id",    // Export specific element by ID
    ExportAreaPage = true,               // Export page area (default: false)
    ExportAreaDrawing = false,           // Export drawing area (default: true)
    ResourcesDir = "path/to/resources"   // Directory for external resources
};
```

## ResvgOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Width` | `int?` | `null` | Output width in pixels |
| `Height` | `int?` | `null` | Output height in pixels |
| `Zoom` | `float?` | `null` | Scale factor |
| `Dpi` | `int` | `96` | Dots per inch |
| `UseFonts` | `byte[][]?` | `null` | Custom fonts as byte arrays |
| `UseFontFile` | `string?` | `null` | Path to font file |
| `UseFontDir` | `string?` | `null` | Path to fonts directory |
| `SkipSystemFonts` | `bool` | `false` | Skip system fonts |
| `Background` | `string?` | `null` | Background color |
| `ExportId` | `string?` | `null` | Export specific element by ID |
| `ExportAreaPage` | `bool` | `false` | Export page area |
| `ExportAreaDrawing` | `bool` | `true` | Export drawing area |
| `ResourcesDir` | `string?` | `null` | Resources directory path |