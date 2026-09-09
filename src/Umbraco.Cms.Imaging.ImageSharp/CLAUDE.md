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

### Project Structure (10 source files)

```
Umbraco.Cms.Imaging.ImageSharp/
├── ImageSharpComposer.cs                    # Auto-registration via IComposer
├── UmbracoBuilderExtensions.cs              # DI setup and middleware configuration
├── ConfigureImageSharpMiddlewareOptions.cs  # Middleware options (caching, HMAC, size limits, decode throttle)
├── ConfigurePhysicalFileSystemCacheOptions.cs # File cache location
├── ImageProcessingThrottleMiddleware.cs     # Bounds concurrent processing; owns the slot lifetime
├── ImageProcessingSlot.cs                   # One request's claim on the concurrency limit
├── ImageProcessors/
│   └── CropWebProcessor.cs                  # Custom crop processor with EXIF awareness
└── Media/
    ├── ImageSharpDimensionExtractor.cs      # Extract image dimensions (EXIF-aware)
    ├── ImageSharpImageUrlGenerator.cs       # Generate query string URLs for processing
    └── ImageSharpImageUrlTokenGenerator.cs  # Re-sign image URLs after an HMAC key rotation
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
        },
        "Memory": {
          "Enabled": true,
          "MaximumPoolSizeMegabytes": 0,
          "MaximumConcurrentProcessing": 0
        }
      }
    }
  }
}
```

### Memory Settings (`ImagingMemorySettings`)

Both numeric values default to `0`, meaning "derive from the memory available to the process"
(`GC.GetGCMemoryInfo().TotalAvailableMemoryBytes`, which honours a container limit).

| Setting | Purpose | Default |
|---------|---------|---------|
| `Enabled` | Master switch for imaging memory management. When `false`, neither the pool cap nor the concurrency bound is applied and ImageSharp's own memory behaviour is left untouched. | `true` |
| `MaximumPoolSizeMegabytes` | Caps the unmanaged buffer pool ImageSharp retains between requests | available / 32, clamped to 16-64 MB |
| `MaximumConcurrentProcessing` | Caps how many images are processed at once | (available / 2) / 64 MB, capped at processor count |

Both bounds are default-on but **conditional**, so an upgrade changes nothing on a host that was
never at risk. Each has its own engagement test, and setting either value explicitly overrides that
test — an operator who names a number gets it.

| Bound | Engages when | Test |
|-------|--------------|------|
| Pool cap | Under 4 GB is available to the process | `RequiresPoolSizeLimit` |
| Concurrency | The memory budget cannot feed as many concurrent decodes as there are processors | `RequiresConcurrencyLimit` |

The two tests deliberately differ. Concurrency is about *peak* — it only needs bounding where memory
is tighter than the core count, since decoding is CPU bound and the processor count caps it
otherwise. The pool cap is about *retention*, and ImageSharp's default there is an eighth of
available memory on **any 64-bit host** — [`GetDefaultMaxPoolSizeBytes`](https://github.com/SixLabors/ImageSharp/blob/v3.1.12/src/ImageSharp/Memory/Allocators/UniformUnmanagedMemoryPoolMemoryAllocator.cs#L156)
returns `total / 8` when `Environment.Is64BitProcess`, and a flat 128 MB otherwise. It is never
disproportionate; it is a problem only in absolute terms, where that eighth competes with the memory
the rest of the site needs. Hence a flat memory threshold rather than a ratio — and note that
reusing `RequiresConcurrencyLimit` for the pool would switch it off on the low-core 2 GB host where
the retention was actually measured.

`Enabled: false` remains the one-setting escape hatch that restores stock ImageSharp behaviour.

Both decisions are logged at startup — at Information when a bound engages, naming the resolved
value, and at Debug when it does not. That log line is the first thing to ask for when diagnosing
either an exit 137 or an unexplained change in image throughput.

**Why these exist**: a source image is decoded at full resolution before any processor runs, and
`ImageSharpMiddleware` only de-duplicates concurrent requests for the *same* URL. A page of distinct
thumbnails therefore decodes every source in parallel, so peak memory is
`concurrent requests x decoded source size` — measured at ~60 MB for one 300x300 thumbnail of a
4000x3000 JPEG. Unbounded, that exhausts a container limit and the process is killed (exit 137).

The concurrency cap is applied in two stages, and requests over it wait rather than being rejected.

1. `ImageProcessingThrottleMiddleware`, registered ahead of `UseImageSharp()` in the pre-pipeline,
   decides whether a request *could* decode: it needs a registered processor command and a format
   that resolves to a configured image format, so an unrelated response like
   `/export.csv?format=xlsx` is left alone. For those that qualify it publishes an
   `ImageProcessingSlot` on `HttpContext.Items` and releases it when the request ends.
2. `ConfigureImageSharpMiddlewareOptions` wires `OnBeforeLoadAsync`, which ImageSharp invokes on a
   cache **miss** only, after the source is resolved and immediately before the decode. That is
   where the wait happens, so a cache hit never queues behind a decode.

Splitting it this way keeps the wait precise while leaving the release somewhere it is guaranteed to
run — the hook has no matching "after" callback, and the middleware's `finally` does. The slot is
held until the request ends rather than freed when processing finishes, because the decoded image
stays in memory while the result is encoded and cached.

**ImageSharp 2.x differs here.** ImageSharp.Web 2.0.2 has no `OnBeforeLoadAsync` (its earliest hook,
`OnParseCommandsAsync`, runs before the cache check), so `Umbraco.Cms.Imaging.ImageSharp2` waits in
the middleware for anything its request filter matches — cache hits included. The two copies of
`ImageProcessingThrottleMiddleware` are therefore *not* interchangeable; the v2 copy is the coarser
fallback.

ImageSharp's own pool default is an eighth of available memory on a 64-bit process, released only on
a gen2 collection and then at most 50% per minute, which leaves a container sitting well above its
working set at rest. This memory is unmanaged, so no `DOTNET_GC*` setting governs it.

### Security: Max Dimension Limits

`ConfigureImageSharpMiddlewareOptions.cs` enforces max dimensions on every request, **regardless of whether HMAC is configured**:
- Width/height requests exceeding `MaxWidth`/`MaxHeight` are **stripped** from the query
- This prevents DoS via excessive image generation

HMAC and the size limits do two separate jobs: HMAC controls *who* may request processing, while `MaxWidth`/`MaxHeight` control *how large* the output may be. The dimension ceiling therefore still applies to validly-signed requests — for legitimate (server-generated) URLs it is a no-op, and it remains a useful backstop if the HMAC secret key ever leaks.

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
