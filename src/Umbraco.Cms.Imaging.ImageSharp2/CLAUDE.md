# Umbraco.Cms.Imaging.ImageSharp2

Image processing library using **ImageSharp 2.x** for backwards compatibility with existing deployments. Use this package only when migrating from older Umbraco versions that depend on ImageSharp 2.x behavior.

**Namespace Note**: Uses `Umbraco.Cms.Imaging.ImageSharp` (same as v3 package) for drop-in replacement - no code changes needed when switching.

---

## 1. Architecture

**Type**: Class Library (NuGet Package)
**Target Framework**: .NET 10.0
**Purpose**: ImageSharp 2.x compatibility layer

### Package Versions (Pinned)

```xml
<!-- From csproj lines 7-8 -->
<PackageReference Include="SixLabors.ImageSharp" VersionOverride="[2.1.11, 3)" />
<PackageReference Include="SixLabors.ImageSharp.Web" VersionOverride="[2.0.2, 3)" />
```

Version constraint `[2.1.11, 3)` means: minimum 2.1.11, below 3.0.

### Project Structure (7 source files)

```
Umbraco.Cms.Imaging.ImageSharp2/
├── ImageSharpComposer.cs                    # Auto-registration via IComposer
├── UmbracoBuilderExtensions.cs              # DI setup and middleware configuration
├── ConfigureImageSharpMiddlewareOptions.cs  # Middleware options (caching, size limits)
├── ConfigurePhysicalFileSystemCacheOptions.cs # File cache location
├── ImageProcessors/
│   └── CropWebProcessor.cs                  # Custom crop processor with EXIF awareness
└── Media/
    ├── ImageSharpDimensionExtractor.cs      # Extract image dimensions (EXIF-aware)
    └── ImageSharpImageUrlGenerator.cs       # Generate query string URLs
```

---

## 2. Key Differences from ImageSharp (3.x)

| Feature | ImageSharp2 (this) | ImageSharp (3.x) |
|---------|-------------------|------------------|
| **Package version** | 2.1.11 - 2.x | 3.x+ |
| **HMAC signing** | Not supported | Supported |
| **WebP default** | Lossy (native) | Lossless (overridden to Lossy) |
| **Cache buster param** | `rnd` only | `rnd` or `v` |
| **API differences** | `Image.Identify(config, stream)` | `Image.Identify(options, stream)` |
| **Size property** | `image.Image.Size()` method | `image.Image.Size` property |

### API Differences in Code

**ImageSharpDimensionExtractor** (`Media/ImageSharpDimensionExtractor.cs:31`):
```csharp
// v2: Direct method call
IImageInfo imageInfo = Image.Identify(_configuration, stream);

// v3: Uses DecoderOptions
ImageInfo imageInfo = Image.Identify(options, stream);
```

**CropWebProcessor** (`ImageProcessors/CropWebProcessor.cs:67`):
```csharp
// v2: Size is a method
Size size = image.Image.Size();

// v3: Size is a property
Size size = image.Image.Size;
```

### Missing Features (vs ImageSharp 3.x)

1. **No HMAC request authorization** - `HMACSecretKey` setting is ignored
2. **No `v` cache buster** - Only `rnd` parameter triggers immutable headers
3. **No WebP encoder override** - Uses default Lossy encoding (no configuration needed)

---

## 3. When to Use This Package

**Use ImageSharp2 when:**
- Migrating from Umbraco versions that used ImageSharp 2.x
- Third-party packages have hard dependency on ImageSharp 2.x
- Need exact byte-for-byte output compatibility with existing cached images

**Use ImageSharp (3.x) when:**
- New installations
- Need HMAC URL signing for security
- Want latest performance improvements

---

## 4. Configuration

Same as ImageSharp 3.x. See `/src/Umbraco.Cms.Imaging.ImageSharp/CLAUDE.md` → Section 3 for full configuration details.

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
| `ConfigureImageSharpMiddlewareOptions.cs` | Size limit enforcement |
| `Media/ImageSharpImageUrlGenerator.cs` | URL generation (no HMAC) |

### Switching Between Packages

To switch from ImageSharp2 to ImageSharp (3.x):
1. Remove `Umbraco.Cms.Imaging.ImageSharp2` package reference
2. Add `Umbraco.Cms.Imaging.ImageSharp` package reference
3. Clear media cache folder (`~/umbraco/Data/TEMP/MediaCache`)
4. No code changes needed (same namespace)

### Getting Help

- **ImageSharp 3.x Documentation**: `/src/Umbraco.Cms.Imaging.ImageSharp/CLAUDE.md`
- **Root Documentation**: `/CLAUDE.md`
- **SixLabors ImageSharp 2.x Docs**: https://docs.sixlabors.com/

---

**This is a backwards-compatibility package. For new projects, use `Umbraco.Cms.Imaging.ImageSharp` (3.x) instead.**
