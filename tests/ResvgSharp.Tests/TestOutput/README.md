# Test Output

This directory contains PNG images generated during test execution to visually verify the ResvgSharp rendering functionality.

## Generated Files

- **minimal-svg.png** - Basic red rectangle SVG rendering
- **custom-dimensions-200x200.png** - SVG rendered at 200x200 pixels
- **zoom-2x.png** - SVG rendered with 2x zoom factor
- **white-background.png** - SVG with white background color
- **dpi-150.png** - SVG rendered at 150 DPI
- **complex-gradient-circle-text.png** - Complex SVG with gradients, shapes, and text
- **inter-font-regular.png** - Text rendered with Inter Regular font
- **multiple-fonts.png** - Text rendered with multiple font files loaded
- **no-system-fonts.png** - Text rendered without system fonts
- **hex-background-colors.png** - SVG with hex color background
- **complex-shapes-gradients.png** - Advanced shapes with radial gradients and patterns
- **high-resolution-800x800.png** - High-resolution 800x800 rendering at 300 DPI
- **text-different-fonts.png** - Various text styles and font variations
- **transparency-opacity.png** - Overlapping shapes with transparency effects

These images are automatically generated when running the test suite and can be used to manually verify that SVG rendering is working correctly across different scenarios.

> **Note**: This directory is excluded from version control via .gitignore