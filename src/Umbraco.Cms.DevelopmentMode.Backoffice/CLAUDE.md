# Umbraco.Cms.DevelopmentMode.Backoffice

Development-time library enabling **InMemoryAuto** ModelsBuilder mode with runtime Razor view compilation. Allows content type changes to instantly regenerate strongly-typed models without application restart.

---

## 1. Architecture

**Type**: Class Library (NuGet Package)
**Target Framework**: .NET 10.0
**Purpose**: Enable hot-reload of ModelsBuilder models during development

### Key Technologies

- **Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation** - Runtime Razor view compilation
- **Microsoft.CodeAnalysis.CSharp** (Roslyn) - Runtime C# compilation
- **AssemblyLoadContext** - Collectible assembly loading/unloading

### Dependencies

- `Umbraco.Web.Common` - Umbraco web infrastructure

### Project Structure (18 source files)

```
Umbraco.Cms.DevelopmentMode.Backoffice/
├── DependencyInjection/
│   ├── BackofficeDevelopmentComposer.cs   # Auto-registration via IComposer
│   └── UmbracoBuilderExtensions.cs        # DI setup (CRITICAL: contains 80-line design overview)
└── InMemoryAuto/
    ├── InMemoryModelFactory.cs            # Core factory - generates/loads models (876 lines)
    ├── InMemoryAssemblyLoadContextManager.cs  # Manages collectible AssemblyLoadContext
    ├── RoslynCompiler.cs                  # Compiles generated C# to DLL
    ├── CollectibleRuntimeViewCompiler.cs  # Custom IViewCompiler for Razor (489 lines)
    ├── UmbracoViewCompilerProvider.cs     # Provides CollectibleRuntimeViewCompiler
    ├── RuntimeCompilationCacheBuster.cs   # Clears Razor caches on model rebuild
    ├── UmbracoRazorReferenceManager.cs    # Manages Roslyn MetadataReferences
    ├── CompilationOptionsProvider.cs      # Mirrors host app compilation settings
    ├── UmbracoAssemblyLoadContext.cs      # Collectible AssemblyLoadContext wrapper
    ├── ChecksumValidator.cs               # Validates precompiled view checksums
    ├── CompilationExceptionFactory.cs     # Creates detailed compilation errors
    ├── UmbracoCompilationException.cs     # Custom exception with CompilationFailures
    ├── ModelsBuilderAssemblyAttribute.cs  # Marks InMemory assemblies
    ├── ModelsBuilderBindingErrorHandler.cs # Handles model binding version mismatches
    ├── InMemoryModelsBuilderModeValidator.cs # Validates runtime mode configuration
    └── ModelsModeConstants.cs             # "InMemoryAuto" constant
```

### Design Patterns

1. **Clone-and-Own Pattern** - ASP.NET Core's RuntimeViewCompiler and related classes are cloned because internal APIs can't be extended
2. **Collectible AssemblyLoadContext** - Enables assembly unloading for hot-reload
3. **FileSystemWatcher** - Monitors `~/umbraco/Data/TEMP/InMemoryAuto/` for changes
4. **Lazy Initialization** - References resolved on first use, not startup

---

## 2. Key Patterns

### Why Clone-and-Own (CRITICAL CONTEXT)

The 80-line comment in `DependencyInjection/UmbracoBuilderExtensions.cs:13-80` explains why this project exists. Key points:

1. **Problem**: ASP.NET Core's `RuntimeViewCompiler` loads assemblies into the default `AssemblyLoadContext`, which can't reference collectible contexts (breaking change in .NET 7)
2. **Failed Solutions**:
   - Reflection to clear caches (unstable, internal APIs)
   - Service wrapping via DI (still loads into wrong context)
3. **Solution**: Clone `RuntimeViewCompiler`, `RazorReferenceManager`, `ChecksumValidator`, and related classes, modifying them to:
   - Load compiled views into the same collectible `AssemblyLoadContext` as models
   - Explicitly add InMemoryAuto models assembly reference during compilation

### InMemoryModelFactory Lifecycle

```
Content Type Changed → Reset() called → _pendingRebuild = true
                                           ↓
Next View Request → EnsureModels() → GetModelsAssembly(forceRebuild: true)
                                           ↓
                    GenerateModelsCode() → RoslynCompiler.CompileToFile()
                                           ↓
                    ReloadAssembly() → InMemoryAssemblyLoadContextManager.RenewAssemblyLoadContext()
                                           ↓
                    RuntimeCompilationCacheBuster.BustCache() → Views recompile with new models
```

### Assembly Caching Strategy

Models are cached to disk for faster boot (`InMemoryModelFactory.cs:400-585`):

1. **Hash Check**: `TypeModelHasher.Hash(typeModels)` creates hash of content type definitions
2. **Cache Files** (in `~/umbraco/Data/TEMP/InMemoryAuto/`):
   - `models.hash` - Current content type hash
   - `models.generated.cs` - Generated source
   - `all.generated.cs` - Combined source with assembly attributes
   - `all.dll.path` - Path to compiled DLL
   - `Compiled/generated.cs{hash}.dll` - Compiled assembly

3. **Boot Sequence**:
   - Check if `models.hash` matches current hash
   - If match, load existing DLL
   - If mismatch or missing, recompile

### Collectible AssemblyLoadContext

```csharp
// InMemoryAssemblyLoadContextManager.cs:29-37
internal void RenewAssemblyLoadContext()
{
    _currentAssemblyLoadContext?.Unload();  // Unload previous
    _currentAssemblyLoadContext = new UmbracoAssemblyLoadContext();  // Create new collectible
}
```

**Key constraint**: No external references to the `AssemblyLoadContext` allowed - prevents unloading.

---

## 3. Error Handling

### Compilation Failures

`CompilationExceptionFactory.cs` creates `UmbracoCompilationException` with:
- Source file content
- Generated code
- Diagnostic messages with line/column positions

Errors logged in `CollectibleRuntimeViewCompiler.cs:383-396` and `InMemoryModelFactory.cs:341-354`.

### Model Binding Errors

`ModelsBuilderBindingErrorHandler.cs` handles version mismatches between:
- View's model type (from compiled view assembly)
- Content's model type (from InMemoryAuto assembly)

Reports detailed error messages including assembly versions.

---

## 4. Security

**Runtime Mode Validation**: `InMemoryModelsBuilderModeValidator.cs` prevents `InMemoryAuto` mode outside `BackofficeDevelopment` runtime mode.

**Temp File Location**: Models compiled to `~/umbraco/Data/TEMP/InMemoryAuto/` - ensure this directory isn't web-accessible.

---

## 5. Edge Cases & Known Issues

### Technical Debt (TODOs)

1. **Circular Reference** - `InMemoryModelFactory.cs:46`:
   ```csharp
   private readonly Lazy<UmbracoServices> _umbracoServices; // TODO: this is because of circular refs :(
   ```

2. **DynamicMethod Performance** - `InMemoryModelFactory.cs:698-701`:
   ```csharp
   // TODO: use Core's ReflectionUtilities.EmitCtor !!
   // Yes .. DynamicMethod is uber slow
   ```

### Race Conditions

`InMemoryModelFactory.cs:800-809` - FileSystemWatcher can cause race conditions on slow cloud filesystems. Own file changes are always ignored.

### Unused Assembly Cleanup

`InMemoryModelFactory.cs:587-612` - `TryDeleteUnusedAssemblies` may fail with `UnauthorizedAccessException` if files are locked. Cleanup retried on next rebuild.

### Reflection for Cache Clearing

`RuntimeCompilationCacheBuster.cs:50-51` uses reflection to call internal `RazorViewEngine.ClearCache()`:
```csharp
Action<RazorViewEngine>? clearCacheMethod = ReflectionUtilities.EmitMethod<Action<RazorViewEngine>>("ClearCache");
```

---

## 6. Configuration

**Enable InMemoryAuto mode** (appsettings.json):
```json
{
  "Umbraco": {
    "CMS": {
      "Runtime": {
        "Mode": "BackofficeDevelopment"
      },
      "ModelsBuilder": {
        "ModelsMode": "InMemoryAuto"
      }
    }
  }
}
```

**Requirements**:
- `RuntimeMode` must be `BackofficeDevelopment`
- `ModelsMode` must be `InMemoryAuto`

---

## Quick Reference

### Essential Commands

```bash
# Build
dotnet build src/Umbraco.Cms.DevelopmentMode.Backoffice/Umbraco.Cms.DevelopmentMode.Backoffice.csproj

# Pack for NuGet
dotnet pack src/Umbraco.Cms.DevelopmentMode.Backoffice/Umbraco.Cms.DevelopmentMode.Backoffice.csproj -c Release

# Run integration tests
dotnet test tests/Umbraco.Tests.Integration/ --filter "FullyQualifiedName~ModelsBuilder"
```

**Note**: This library has no direct tests - tested via integration tests in `Umbraco.Tests.Integration`.

**Focus areas when modifying**:
- Assembly loading/unloading cycles
- Razor view compilation with models
- Cache invalidation timing
- Model binding error scenarios

### Essential Classes

| Class | Purpose | File |
|-------|---------|------|
| `InMemoryModelFactory` | Core model generation/loading | `InMemoryAuto/InMemoryModelFactory.cs` |
| `CollectibleRuntimeViewCompiler` | Custom Razor compiler | `InMemoryAuto/CollectibleRuntimeViewCompiler.cs` |
| `InMemoryAssemblyLoadContextManager` | Collectible assembly management | `InMemoryAuto/InMemoryAssemblyLoadContextManager.cs` |
| `RuntimeCompilationCacheBuster` | Cache invalidation | `InMemoryAuto/RuntimeCompilationCacheBuster.cs` |
| `UmbracoBuilderExtensions` | DI setup + design rationale | `DependencyInjection/UmbracoBuilderExtensions.cs` |

### Important Files

- **Design Overview**: `DependencyInjection/UmbracoBuilderExtensions.cs:13-80` - READ THIS FIRST
- **Project File**: `Umbraco.Cms.DevelopmentMode.Backoffice.csproj`
- **Model Factory**: `InMemoryAuto/InMemoryModelFactory.cs` - Most complex class

### Cloned Microsoft Code

These files are clones of ASP.NET Core internals - check upstream for updates:
- `ChecksumValidator.cs` - https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Razor.RuntimeCompilation/src/ChecksumValidator.cs
- `UmbracoRazorReferenceManager.cs` - https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs
- `CompilationOptionsProvider.cs` - Partial clone of CSharpCompiler
- `CompilationExceptionFactory.cs` - Partial clone of CompilationFailedExceptionFactory

### Getting Help

- **Root Documentation**: `/CLAUDE.md`
- **Core Patterns**: `/src/Umbraco.Core/CLAUDE.md`
- **Official Docs**: https://docs.umbraco.com/umbraco-cms/reference/configuration/modelsbuildersettings

---

**This library enables hot-reload of content models during development. The core complexity is working around ASP.NET Core's internal Razor compilation APIs. Always read the design overview in `UmbracoBuilderExtensions.cs:13-80` before making changes.**
