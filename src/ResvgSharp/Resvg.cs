using System;
using System.Runtime.InteropServices;
using System.Text;
using ResvgSharp.Exceptions;

namespace ResvgSharp;

public static class Resvg
{
    private const string LibraryName = "resvg_wrapper";

    [StructLayout(LayoutKind.Sequential)]
    private struct RenderOptions
    {
        public int width;
        public int height;
        public float zoom;
        public int dpi;
        [MarshalAs(UnmanagedType.I1)]
        public bool skip_system_fonts;
        public IntPtr background;
        public IntPtr export_id;
        [MarshalAs(UnmanagedType.I1)]
        public bool export_area_page;
        [MarshalAs(UnmanagedType.I1)]
        public bool export_area_drawing;
        public IntPtr resources_dir;
        public IntPtr fonts;
        public IntPtr font_lens;
        public UIntPtr font_count;
        public IntPtr font_file;
        public IntPtr font_dir;
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int render_svg_to_png_with_options(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string svg_data,
        ref RenderOptions options,
        out IntPtr out_buf,
        out UIntPtr out_len
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void free_png_buffer(IntPtr buffer, UIntPtr len);

    public static byte[] RenderToPng(string svg, ResvgOptions? options = null)
    {
        if (string.IsNullOrEmpty(svg))
        {
            throw new ArgumentNullException(nameof(svg));
        }

        options ??= new ResvgOptions();

        var nativeOptions = new RenderOptions
        {
            width = options.Width ?? -1,
            height = options.Height ?? -1,
            zoom = options.Zoom ?? 0.0f,
            dpi = options.Dpi,
            skip_system_fonts = options.SkipSystemFonts,
            export_area_page = options.ExportAreaPage,
            export_area_drawing = options.ExportAreaDrawing,
            background = IntPtr.Zero,
            export_id = IntPtr.Zero,
            resources_dir = IntPtr.Zero,
            fonts = IntPtr.Zero,
            font_lens = IntPtr.Zero,
            font_count = UIntPtr.Zero,
            font_file = IntPtr.Zero,
            font_dir = IntPtr.Zero
        };

        IntPtr backgroundPtr = IntPtr.Zero;
        IntPtr exportIdPtr = IntPtr.Zero;
        IntPtr resourcesDirPtr = IntPtr.Zero;
        IntPtr fontFilePtr = IntPtr.Zero;
        IntPtr fontDirPtr = IntPtr.Zero;
        IntPtr[] fontPtrs = Array.Empty<IntPtr>();
        IntPtr fontArrayPtr = IntPtr.Zero;
        IntPtr fontLensPtr = IntPtr.Zero;

        try
        {
            if (!string.IsNullOrEmpty(options.Background))
            {
                backgroundPtr = Marshal.StringToCoTaskMemUTF8(options.Background);
                nativeOptions.background = backgroundPtr;
            }

            if (!string.IsNullOrEmpty(options.ExportId))
            {
                exportIdPtr = Marshal.StringToCoTaskMemUTF8(options.ExportId);
                nativeOptions.export_id = exportIdPtr;
            }

            if (!string.IsNullOrEmpty(options.ResourcesDir))
            {
                resourcesDirPtr = Marshal.StringToCoTaskMemUTF8(options.ResourcesDir);
                nativeOptions.resources_dir = resourcesDirPtr;
            }

            if (!string.IsNullOrEmpty(options.UseFontFile))
            {
                fontFilePtr = Marshal.StringToCoTaskMemUTF8(options.UseFontFile);
                nativeOptions.font_file = fontFilePtr;
            }

            if (!string.IsNullOrEmpty(options.UseFontDir))
            {
                fontDirPtr = Marshal.StringToCoTaskMemUTF8(options.UseFontDir);
                nativeOptions.font_dir = fontDirPtr;
            }

            if (options.UseFonts != null && options.UseFonts.Length > 0)
            {
                fontPtrs = new IntPtr[options.UseFonts.Length];
                var fontLens = new UIntPtr[options.UseFonts.Length];

                for (int i = 0; i < options.UseFonts.Length; i++)
                {
                    var fontData = options.UseFonts[i];
                    if (fontData == null || fontData.Length == 0)
                    {
                        throw new ResvgFontLoadException("Font data cannot be null or empty");
                    }

                    fontPtrs[i] = Marshal.AllocHGlobal(fontData.Length);
                    Marshal.Copy(fontData, 0, fontPtrs[i], fontData.Length);
                    fontLens[i] = new UIntPtr((uint)fontData.Length);
                }

                fontArrayPtr = Marshal.AllocHGlobal(IntPtr.Size * fontPtrs.Length);
                Marshal.Copy(fontPtrs, 0, fontArrayPtr, fontPtrs.Length);

                fontLensPtr = Marshal.AllocHGlobal(UIntPtr.Size * fontLens.Length);
                for (int i = 0; i < fontLens.Length; i++)
                {
                    Marshal.WriteIntPtr(fontLensPtr, i * UIntPtr.Size, (IntPtr)fontLens[i]);
                }

                nativeOptions.fonts = fontArrayPtr;
                nativeOptions.font_lens = fontLensPtr;
                nativeOptions.font_count = new UIntPtr((uint)options.UseFonts.Length);
            }

            IntPtr pngBuffer;
            UIntPtr pngLength;
            
            int result = render_svg_to_png_with_options(svg, ref nativeOptions, out pngBuffer, out pngLength);

            if (result != 0)
            {
                throw result switch
                {
                    1 => new ResvgParseException("Failed to parse SVG"),
                    2 => new ResvgPngRenderException("Failed to render PNG"),
                    3 => new ResvgFontLoadException("Failed to load fonts"),
                    4 => new OutOfMemoryException("Memory allocation failed"),
                    _ => new ResvgException($"Unknown error: {result}")
                };
            }

            try
            {
                var pngData = new byte[(int)pngLength];
                Marshal.Copy(pngBuffer, pngData, 0, (int)pngLength);
                return pngData;
            }
            finally
            {
                free_png_buffer(pngBuffer, pngLength);
            }
        }
        finally
        {
            if (backgroundPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(backgroundPtr);
            if (exportIdPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(exportIdPtr);
            if (resourcesDirPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(resourcesDirPtr);
            if (fontFilePtr != IntPtr.Zero) Marshal.FreeCoTaskMem(fontFilePtr);
            if (fontDirPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(fontDirPtr);

            foreach (var ptr in fontPtrs)
            {
                if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
            }

            if (fontArrayPtr != IntPtr.Zero) Marshal.FreeHGlobal(fontArrayPtr);
            if (fontLensPtr != IntPtr.Zero) Marshal.FreeHGlobal(fontLensPtr);
        }
    }
}