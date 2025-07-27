use std::ffi::CStr;
use std::os::raw::{c_char, c_int};
use std::slice;
use std::path::Path;
use resvg::tiny_skia;
use resvg::usvg::{fontdb::Database, Options, Tree};

#[repr(C)]
pub struct RenderOptions {
    pub width: c_int,
    pub height: c_int,
    pub zoom: f32,
    pub dpi: c_int,
    pub skip_system_fonts: bool,
    pub background: *const c_char,
    pub export_id: *const c_char,
    pub export_area_page: bool,
    pub export_area_drawing: bool,
    pub resources_dir: *const c_char,
    pub fonts: *const *const u8,
    pub font_lens: *const usize,
    pub font_count: usize,
    pub font_file: *const c_char,
    pub font_dir: *const c_char,
}

unsafe fn c_str_to_string(ptr: *const c_char) -> Option<String> {
    if ptr.is_null() {
        None
    } else {
        CStr::from_ptr(ptr).to_str().ok().map(|s| s.to_string())
    }
}

#[no_mangle]
pub extern "C" fn render_svg_to_png_with_options(
    svg_data: *const c_char,
    options: *const RenderOptions,
    out_buf: *mut *mut u8,
    out_len: *mut usize,
) -> c_int {
    unsafe {
        if svg_data.is_null() || options.is_null() || out_buf.is_null() || out_len.is_null() {
            return 4;
        }

        let svg_str = match CStr::from_ptr(svg_data).to_str() {
            Ok(s) => s,
            Err(_) => return 1,
        };

        let opts = &*options;
        
        let mut fontdb = Database::new();
        
        if !opts.skip_system_fonts {
            fontdb.load_system_fonts();
        }
        
        if opts.font_count > 0 && !opts.fonts.is_null() && !opts.font_lens.is_null() {
            let font_ptrs = slice::from_raw_parts(opts.fonts, opts.font_count);
            let font_lens = slice::from_raw_parts(opts.font_lens, opts.font_count);
            
            for i in 0..opts.font_count {
                let font_data = slice::from_raw_parts(font_ptrs[i], font_lens[i]);
                fontdb.load_font_data(font_data.to_vec());
            }
        }
        
        if let Some(font_file) = c_str_to_string(opts.font_file) {
            let _ = fontdb.load_font_file(&font_file);
        }
        
        if let Some(font_dir) = c_str_to_string(opts.font_dir) {
            fontdb.load_fonts_dir(&font_dir);
        }
        
        let mut usvg_opts = Options::default();
        usvg_opts.fontdb = std::sync::Arc::new(fontdb);
        
        if opts.dpi > 0 {
            usvg_opts.dpi = opts.dpi as f32;
        }
        
        if let Some(resources_dir) = c_str_to_string(opts.resources_dir) {
            usvg_opts.resources_dir = Some(Path::new(&resources_dir).to_path_buf());
        }
        
        let tree = match Tree::from_str(svg_str, &usvg_opts) {
            Ok(tree) => tree,
            Err(_) => return 1,
        };
        
        let size = tree.size();
        let mut target_width = size.width() as u32;
        let mut target_height = size.height() as u32;
        
        if opts.zoom > 0.0 {
            target_width = (target_width as f32 * opts.zoom) as u32;
            target_height = (target_height as f32 * opts.zoom) as u32;
        } else {
            if opts.width > 0 && opts.height > 0 {
                target_width = opts.width as u32;
                target_height = opts.height as u32;
            } else if opts.width > 0 {
                let ratio = opts.width as f32 / size.width();
                target_width = opts.width as u32;
                target_height = (size.height() * ratio) as u32;
            } else if opts.height > 0 {
                let ratio = opts.height as f32 / size.height();
                target_width = (size.width() * ratio) as u32;
                target_height = opts.height as u32;
            }
        }
        
        target_width = target_width.clamp(1, 16384);
        target_height = target_height.clamp(1, 16384);
        
        let mut pixmap = match tiny_skia::Pixmap::new(target_width, target_height) {
            Some(pixmap) => pixmap,
            None => return 4,
        };
        
        if let Some(bg_str) = c_str_to_string(opts.background) {
            // Parse color manually for older API
            if let Some(color) = parse_color(&bg_str) {
                pixmap.fill(color);
            }
        }
        
        let transform = tiny_skia::Transform::from_scale(
            target_width as f32 / size.width(),
            target_height as f32 / size.height(),
        );
        
        resvg::render(&tree, transform, &mut pixmap.as_mut());
        
        let png_data = match pixmap.encode_png() {
            Ok(data) => data,
            Err(_) => return 2,
        };
        
        let buffer = malloc(png_data.len()) as *mut u8;
        if buffer.is_null() {
            return 4;
        }
        
        std::ptr::copy_nonoverlapping(png_data.as_ptr(), buffer, png_data.len());
        *out_buf = buffer;
        *out_len = png_data.len();
        
        0
    }
}

#[no_mangle]
pub extern "C" fn free_png_buffer(buffer: *mut u8, len: usize) {
    if !buffer.is_null() && len > 0 {
        unsafe {
            free(buffer as *mut std::ffi::c_void);
        }
    }
}

fn parse_color(s: &str) -> Option<tiny_skia::Color> {
    // Simple color parsing for common cases
    match s {
        "red" => Some(tiny_skia::Color::from_rgba8(255, 0, 0, 255)),
        "green" => Some(tiny_skia::Color::from_rgba8(0, 255, 0, 255)),
        "blue" => Some(tiny_skia::Color::from_rgba8(0, 0, 255, 255)),
        "white" => Some(tiny_skia::Color::from_rgba8(255, 255, 255, 255)),
        "black" => Some(tiny_skia::Color::from_rgba8(0, 0, 0, 255)),
        _ => {
            // Try to parse hex color
            if s.starts_with('#') {
                let hex = s.trim_start_matches('#');
                if hex.len() == 6 {
                    if let (Ok(r), Ok(g), Ok(b)) = (
                        u8::from_str_radix(&hex[0..2], 16),
                        u8::from_str_radix(&hex[2..4], 16),
                        u8::from_str_radix(&hex[4..6], 16)
                    ) {
                        return Some(tiny_skia::Color::from_rgba8(r, g, b, 255));
                    }
                } else if hex.len() == 3 {
                    if let (Ok(r), Ok(g), Ok(b)) = (
                        u8::from_str_radix(&hex[0..1], 16),
                        u8::from_str_radix(&hex[1..2], 16),
                        u8::from_str_radix(&hex[2..3], 16)
                    ) {
                        return Some(tiny_skia::Color::from_rgba8(r * 17, g * 17, b * 17, 255));
                    }
                }
            }
            None
        }
    }
}

#[cfg(target_os = "windows")]
extern "C" {
    fn malloc(size: usize) -> *mut std::ffi::c_void;
    fn free(ptr: *mut std::ffi::c_void);
}

#[cfg(unix)]
extern "C" {
    fn malloc(size: usize) -> *mut std::ffi::c_void;
    fn free(ptr: *mut std::ffi::c_void);
}