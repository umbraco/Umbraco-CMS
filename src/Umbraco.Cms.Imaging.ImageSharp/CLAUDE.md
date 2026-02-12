# Umbraco.Cms.Imaging.ImageSharp

Image processing library using **ImageSharp 3.x** and **ImageSharp.Web** for on-the-fly image manipulation, resizing, cropping, and caching.

---

## 1. Architecture

**Type**: Class Library (NuGet Package)
**Target Framework**: .NET 10.0
**Purpose**: Provide image manipulation via query string parameters

### Key Technologies

- **SixLabors.ImageSharp** - Image processing library
- **SixLabors.ImageSharp.Web** - ASP.NET Core middleware for query string-based image manipulation

### Dependencies

- `Umbraco.Web.Common` - Web infrastructure

### Project Structure (7 source files)

```
Umbraco.Cms.Imaging.ImageSharp/
├── ImageSharpComposer.cs                    # Auto-registration via IComposer
├── UmbracoBuilderExtensions.cs              # DI setup and middleware configuration
├── ConfigureImageSharpMiddlewareOptions.cs  # Middleware options (caching, HMAC, size limits)
├── ConfigurePhysicalFileSystemCacheOptions.cs # File cache location
├── ImageProcessors/
│   └── CropWebProcessor.cs                  # Custom crop processor with EXIF awareness
└── Media/
    ├── ImageSharpDimensionExtractor.cs      # Extract image dimensions (EXIF-aware)
    └── ImageSharpImageUrlGenerator.cs       # Generate query string URLs for processing
```

### Relationship to ImageSharp2

Two imaging packages exist:
- **Umbraco.Cms.Imaging.ImageSharp** (this package) - Uses ImageSharp 3.x (default)
- **Umbraco.Cms.Imaging.ImageSharp2** - Uses ImageSharp 2.x for backwards compatibility

**Key difference**: ImageSharp 3.x WebP encoder defaults to Lossless (10x larger files), so this package explicitly sets `WebpFileFormatType.Lossy` at `ConfigureImageSharpMiddlewareOptions.cs:108-115`.

---

## 2. Key Patterns

### Query String Image Processing

Images are processed via URL query parameters handled by ImageSharp.Web middleware:

| Parameter | Purpose | Example |
|-----------|---------|---------|
| `width` / `height` | Resize dimensions | `?width=800&height=600` |
| `mode` | Crop mode (pad, crop, stretch, etc.) | `?mode=crop` |
| `anchor` | Crop anchor position | `?anchor=center` |
| `cc` | Crop coordinates (custom) | `?cc=0.1,0.1,0.1,0.1` |
| `rxy` | Focal point | `?rxy=0.5,0.3` |
| `format` | Output format | `?format=webp` |
| `quality` | Compression quality | `?quality=80` |

### Pipeline Integration

ImageSharp middleware runs **before** static files in `UmbracoBuilderExtensions.cs:44-50`:
```csharp
options.AddFilter(new UmbracoPipelineFilter(nameof(ImageSharpComposer))
{
    PrePipeline = prePipeline => prePipeline.UseImageSharp()
});
```

This ensures query strings are processed before serving static files.

### EXIF Orientation Handling

Both `ImageSharpDimensionExtractor` and `CropWebProcessor` account for EXIF rotation:

```csharp
// ImageSharpDimensionExtractor.cs:42-44 - Swap width/height for rotated images
size = IsExifOrientationRotated(imageInfo)
    ? new Size(imageInfo.Height, imageInfo.Width)
    : new Size(imageInfo.Width, imageInfo.Height);
```

```csharp
// CropWebProcessor.cs:64-65 - Transform crop coordinates for EXIF orientation
Vector2 xy1 = ExifOrientationUtilities.Transform(new Vector2(left, top), Vector2.Zero, Vector2.One, orientation);
```

### HMAC Request Authorization

When `HMACSecretKey` is configured, URLs are signed to prevent abuse (`ImageSharpImageUrlGenerator.cs:121-131`):
```csharp
if (_options.HMACSecretKey.Length != 0 && _requestAuthorizationUtilities is not null)
{
    var token = _requestAuthorizationUtilities.ComputeHMAC(uri, CommandHandling.Sanitize);
    queryString.Add(RequestAuthorizationUtilities.TokenCommand, token);
}
```

---

## 3. Configuration

### ImagingSettings (appsettings.json)

```json
{
  "Umbraco": {
    "CMS": {
      "Imaging": {
        "HMACSecretKey": "",
        "Cache": {
          "BrowserMaxAge": "7.00:00:00",
          "CacheMaxAge": "365.00:00:00",
          "CacheHashLength": 12,
          "CacheFolder": "~/umbraco/Data/TEMP/MediaCache",
          "CacheFolderDepth": 8
        },
        "Resize": {
          "MaxWidth": 5000,
          "MaxHeight": 5000
        }
      }
    }
  }
}
```

### Security: Size Limits Without HMAC

When HMAC is not configured, `ConfigureImageSharpMiddlewareOptions.cs:46-83` enforces max dimensions:
- Width/height requests exceeding `MaxWidth`/`MaxHeight` are **stripped** from the query
- This prevents DoS via excessive image generation

When HMAC **is** configured, size validation is skipped (trusted requests).

### Cache Busting

Query parameters `rnd` or `v` trigger immutable cache headers (`ConfigureImageSharpMiddlewareOptions.cs:86-106`):
- Disables `MustRevalidate`
- Adds `immutable` directive

---

## 4. Core Interfaces Implemented

| Interface | Implementation | Purpose |
|-----------|----------------|---------|
| `IImageDimensionExtractor` | `ImageSharpDimensionExtractor` | Extract width/height from streams |
| `IImageUrlGenerator` | `ImageSharpImageUrlGenerator` | Generate manipulation URLs |
| `IImageWebProcessor` | `CropWebProcessor` | Custom crop with `cc` parameter |

---

## 5. Edge Cases

### WebP Encoding Change (ImageSharp 3.x)

`ConfigureImageSharpMiddlewareOptions.cs:108-115` - ImageSharp 3.x defaults WebP to Lossless for PNGs, creating ~10x larger files. This is overridden:
```csharp
options.Configuration.ImageFormatsManager.SetEncoder(
    WebpFormat.Instance,
    new WebpEncoder { FileFormat = WebpFileFormatType.Lossy });
```

### Crop Coordinates Format

`CropWebProcessor.cs:50-56` - The `cc` parameter expects 4 values as distances from edges:
- Format: `left,top,right,bottom` (0-1 range, percentages)
- Right/bottom values are **distance from** those edges, not coordinates
- Zero values (`0,0,0,0`) are ignored (no crop)

### Supported File Types

Dynamically determined from ImageSharp configuration (`ImageSharpDimensionExtractor.cs:24`):
```csharp
SupportedImageFileTypes = configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray();
```

Default includes: jpg, jpeg, png, gif, bmp, webp, tiff, etc.

---

## Quick Reference

### Essential Commands

```bash
# Build
dotnet build src/Umbraco.Cms.Imaging.ImageSharp/Umbraco.Cms.Imaging.ImageSharp.csproj

# Run tests
dotnet test tests/Umbraco.Tests.UnitTests/ --filter "FullyQualifiedName~ImageSharp"
```

### Key Files

| File | Purpose |
|------|---------|
| `UmbracoBuilderExtensions.cs` | DI registration and pipeline setup |
| `ConfigureImageSharpMiddlewareOptions.cs` | Middleware config (caching, HMAC, size limits) |
| `CropWebProcessor.cs` | Custom `cc` crop parameter |
| `ImageSharpImageUrlGenerator.cs` | URL generation with HMAC signing |

### URL Examples

```
# Basic resize
/media/image.jpg?width=800

# Crop to aspect ratio with focal point
/media/image.jpg?width=800&height=600&mode=crop&rxy=0.5,0.3

# Custom crop coordinates (10% from each edge)
/media/image.jpg?cc=0.1,0.1,0.1,0.1

# Format conversion with quality
/media/image.jpg?format=webp&quality=80

# Cache busted (immutable headers)
/media/image.jpg?width=800&v=abc123
```

### Getting Help

- **Root Documentation**: `/CLAUDE.md`
- **ImageSharp Docs**: https://docs.sixlabors.com/
- **Umbraco Imaging**: https://docs.umbraco.com/umbraco-cms/reference/configuration/imagingsettings

---

**This library provides query string-based image processing. Key concerns are EXIF orientation handling, WebP encoding defaults, and HMAC security for public-facing sites.**
