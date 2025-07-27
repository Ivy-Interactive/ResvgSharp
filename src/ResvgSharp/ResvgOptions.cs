namespace ResvgSharp;

public class ResvgOptions
{
    public int? Width { get; set; }
    
    public int? Height { get; set; }
    
    public float? Zoom { get; set; }
    
    public int Dpi { get; set; } = 96;
    
    public byte[][]? UseFonts { get; set; }
    
    public string? UseFontFile { get; set; }
    
    public string? UseFontDir { get; set; }
    
    public bool SkipSystemFonts { get; set; } = false;
    
    public string? Background { get; set; }
    
    public string? ExportId { get; set; }
    
    public bool ExportAreaPage { get; set; } = false;
    
    public bool ExportAreaDrawing { get; set; } = true;
    
    public string? ResourcesDir { get; set; }
}