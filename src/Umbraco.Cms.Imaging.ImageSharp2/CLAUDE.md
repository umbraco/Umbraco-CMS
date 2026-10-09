# Umbraco.Cms.Imaging.ImageSharp2

Image processing library using **ImageSharp 2.x** for backwards compatibility with existing deployments. Use this package only when migrating from older Umbraco versions that depend on ImageSharp 2.x behavior.

**Namespace Note**: Uses `Umbraco.Cms.Imaging.ImageSharp` (same as the v4 package) for drop-in replacement - no code changes needed when switching.

---

## 1. Architecture

**Type**: Class Library (NuGet Package)
**Target Framework**: .NET 11.0
**Purpose**: ImageSharp 2.x compatibility layer

### Package Versions (Pinned)

```xml
<!-- From csproj lines 7-8 -->
<PackageReference Include="SixLabors.ImageSharp" VersionOverride="[2.1.13, 3)" />
<PackageReference Include="SixLabors.ImageSharp.Web" VersionOverride="[2.0.2, 3)" />
```

Version constraint `[2.1.13, 3)` means: minimum 2.1.13, below 3.0.

### Project Structure (7 source files, plus 5 linked from the 3.x package)

```
Umbraco.Cms.Imaging.ImageSharp2/
├── ImageSharpComposer.cs                    # Auto-registration via IComposer
├── UmbracoBuilderExtensions.cs              # DI setup, memory bounds and middleware configuration
├── ConfigureImageSharpMiddlewareOptions.cs  # Middleware options (caching, size limits, decode slot release)
├── ImageProcessingThrottleMiddleware.cs     # Bounds concurrent processing (2.x variant: waits before the pipeline)
├── ImageProcessors/
│   └── CropWebProcessor.cs                  # Custom crop processor with EXIF awareness
└── Media/
    ├── ImageSharpDimensionExtractor.cs      # Extract image dimensions (EXIF-aware)
    └── ImageSharpImageUrlGenerator.cs       # Generate query string URLs
```

Five more files are compiled in from the 3.x package as linked sources rather than copied, because
nothing in them touches an API that differs between the two majors. The linked-source group in the
csproj is the list. Edit them in the 3.x project; both packages pick up the change.

---

## 2. Key Differences from ImageSharp (4.x)

| Feature | ImageSharp2 (this) | ImageSharp (4.x) |
|---------|-------------------|------------------|
| **Package version** | 2.1.13 - 2.x | 4.x |
| **HMAC signing** | Not supported | Supported |
| **WebP default** | Lossy (native) | Lossless (overridden to Lossy) |
| **Cache buster param** | `rnd` only | `rnd` or `v` |
| **API differences** | `Image.Identify(config, stream)` | `Image.Identify(options, stream)` |
| **Size property** | `image.Image.Size()` method | `image.Image.Size` property |
| **Decode throttle** | Waits before the pipeline, cache hits included | Waits in `OnBeforeLoadAsync`, cache misses only |

### API Differences in Code

**ImageSharpDimensionExtractor** (`Media/ImageSharpDimensionExtractor.cs:31`):
```csharp
// v2: Direct method call
IImageInfo imageInfo = Image.Identify(_configuration, stream);

// v4: Uses DecoderOptions
ImageInfo imageInfo = Image.Identify(options, stream);
```

**CropWebProcessor** (`ImageProcessors/CropWebProcessor.cs:67`):
```csharp
// v2: Size is a method
Size size = image.Image.Size();

// v4: Size is a property
Size size = image.Image.Size;
```

### Missing Features (vs ImageSharp 4.x)

1. **No HMAC request authorization** - `HMACSecretKey` setting is ignored
2. **No `v` cache buster** - Only `rnd` parameter triggers immutable headers
3. **No WebP encoder override** - Uses default Lossy encoding (no configuration needed)
4. **No `OnBeforeLoadAsync` hook** - The decode throttle waits before the pipeline instead (see below)

### Image Processing Memory

Both packages bound image processing memory the same way, from the linked `ImageProcessingMemory`:
a pool cap, a single-image ceiling and a concurrency limit, all off by default until v19 and all
governed by `Umbraco:CMS:Imaging:Memory`. The derivation, the engagement tests and the startup
logging are documented once, in the 3.x package's CLAUDE.md under "Memory Settings".

The one difference is where a gated request **waits**. ImageSharp.Web 2.0.2 has no
`OnBeforeLoadAsync`, so this package's `ImageProcessingThrottleMiddleware` takes its place before
the downstream pipeline runs - a cache hit, or a missing source falling through to a 404, waits as
much as a decode does. The **release** is shared: `OnProcessedAsync` exists in 2.x, so
`ConfigureImageSharpMiddlewareOptions` gives the place back as soon as the decoded image is
disposed, with the end of the request as the backstop.

One consequence of having no decode hook: if ImageSharp.Web retries a request because a freshly
cached result could not be read back, the second decode runs after the place has been released and
nothing takes it again. The 3.x package re-enters its decode hook on a retry and re-acquires.

---

## 3. When to Use This Package

**Use ImageSharp2 when:**
- Migrating from Umbraco versions that used ImageSharp 2.x
- Third-party packages have hard dependency on ImageSharp 2.x
- Need exact byte-for-byte output compatibility with existing cached images
- Prefer to avoid ImageSharp's license change in v3+

**Use ImageSharp (4.x) when:**
- New installations (this is the default, referenced by the `Umbraco.Cms` meta-package)
- Need HMAC URL signing for security
- Want latest performance improvements

---

## 4. Configuration

Same as ImageSharp 4.x. See `/src/Umbraco.Cms.Imaging.ImageSharp/CLAUDE.md` → Section 3 for full configuration details.

**Key difference**: `HMACSecretKey` setting exists but is **ignored** in this package (no HMAC support in v2).

---

## Quick Reference

### Essential Commands

```bash
# Build
dotnet build src/Umbraco.Cms.Imaging.ImageSharp2/Umbraco.Cms.Imaging.ImageSharp2.csproj

# Run tests
dotnet test tests/Umbraco.Tests.UnitTests/ --filter "FullyQualifiedName~ImageSharp"
```

### Key Files

| File | Purpose |
|------|---------|
| `Umbraco.Cms.Imaging.ImageSharp2.csproj` | Version constraints (lines 7-8) |
| `ConfigureImageSharpMiddlewareOptions.cs` | Size limit enforcement; releases the decode slot |
| `ImageProcessingThrottleMiddleware.cs` | Concurrency gate (waits before the pipeline) |
| `Media/ImageSharpImageUrlGenerator.cs` | URL generation (no HMAC) |

### Switching Between Packages

To switch from ImageSharp2 to ImageSharp (4.x):
1. Remove `Umbraco.Cms.Imaging.ImageSharp2` package reference
2. Add `Umbraco.Cms.Imaging.ImageSharp` package reference
3. Clear media cache folder (`~/umbraco/Data/TEMP/MediaCache`)
4. No code changes needed (same namespace)

### Getting Help

- **ImageSharp 4.x Documentation**: `/src/Umbraco.Cms.Imaging.ImageSharp/CLAUDE.md`
- **Root Documentation**: `/CLAUDE.md`
- **SixLabors ImageSharp 2.x Docs**: https://docs.sixlabors.com/

---

**This is a backwards-compatibility package. For new projects, use `Umbraco.Cms.Imaging.ImageSharp` (4.x) instead.**
