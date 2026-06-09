# Package Asset Cache-Busting (per-package hash) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Give each package's `/App_Plugins` importmap assets an automatic, per-package cache-buster (`?umb__rnd=<hash>`) derived from the package's `version`, so package authors can ship updates to their non-bundler-hashed entrypoints without manual cache-busting — covering ADO Task #68840 under PBI #68839.

**Architecture:** A small Core helper computes a per-package hash (`version` → SHA1, falling back to Umbraco's global cache-bust hash when absent) and stamps it onto `/App_Plugins`-rooted importmap URLs only. Stamping happens in `PackageManifestService.GetPackageManifestImportmapAsync` **before** the cross-package merge, where each package's `version` and the new `disableCacheBusting` opt-out are still available. The `%CACHE_BUSTER%` token and the backoffice's own `/umbraco/backoffice/<hash>` path-busting are left completely untouched (auto-stamp skips token-bearing URLs and only touches `/App_Plugins`).

**Tech Stack:** .NET 10, C# 12, NUnit + Moq (Umbraco.Tests.UnitTests), TypeScript (backoffice JSON schema).

**Scope decisions (locked, see memory `ado-68840-package-cache-busting`):**
- Delivery mechanism: **query-string** `?umb__rnd=<hash>` (user's call; CDN caveat goes in the sibling docs/headers task).
- Fallback when no `version`: **global Umbraco hash** (current behaviour).
- Target: **importmap entries only**, restricted to `/App_Plugins`-rooted paths. Extension asset-URL auto-stamping is **out of scope** (would require traversing untyped `Extensions` JSON for path-bearing keys across every extension kind — fragile; the importmap entry is the entrypoint the task targets). `%CACHE_BUSTER%` remains the opt-in for any other path and is unchanged (resolves to the global hash as today).
- Single-server assumption: hashing the version string is sufficient.

---

## File Structure

**Create:**
- `src/Umbraco.Core/Manifest/PackageManifestCacheBuster.cs` — static helper: per-package hash resolution + URL stamping rules.
- `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Manifest/PackageManifestCacheBusterTests.cs` — unit tests for the helper.
- `tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Manifest/PackageManifestServiceTests.cs` — unit tests for importmap stamping (create if absent; extend if present).

**Modify:**
- `src/Umbraco.Core/Manifest/PackageManifest.cs` — add `DisableCacheBusting` property.
- `src/Umbraco.Web.UI.Client/src/json-schema/umbraco-package-schema.ts` — add `disableCacheBusting?: boolean`.
- `src/Umbraco.Infrastructure/Manifest/PackageManifestService.cs` — inject `IHostingEnvironment` + `IUmbracoVersion`; stamp importmap per-package before merge.

**Left untouched (verified non-regressing):**
- `HtmlHelperBackOfficeExtensions.BackOfficeImportMapScriptAsync` — its `%CACHE_BUSTER%` and `/umbraco/backoffice` replaces still work; stamped values never contain the token and never start with `/umbraco/backoffice`.
- `ManifestControllerBase.ReplaceCacheBusterTokens` and the three manifest controllers — token → global hash, unchanged.
- `UrlHelperExtensions.GetCacheBustHash` — unchanged (the service mirrors its 4-line computation privately; intentional, near-zero drift risk).

---

### Task 1: Add `disableCacheBusting` to the package manifest model + schema

**Files:**
- Modify: `src/Umbraco.Core/Manifest/PackageManifest.cs`
- Modify: `src/Umbraco.Web.UI.Client/src/json-schema/umbraco-package-schema.ts:42` (after `allowPublicAccess`)

- [ ] **Step 1: Add the property to `PackageManifest`**

In `src/Umbraco.Core/Manifest/PackageManifest.cs`, add after the `AllowPublicAccess` property (keep the existing positive-boolean style; default `false` so absence = busting on):

```csharp
    /// <summary>
    ///     Gets or sets a value indicating whether automatic cache-busting of this package's
    ///     <c>/App_Plugins</c> importmap assets is disabled. When <c>false</c> (default), Umbraco appends a
    ///     per-package <c>?umb__rnd</c> token derived from <see cref="Version"/> to the package's importmap URLs.
    /// </summary>
    public bool DisableCacheBusting { get; set; }
```

- [ ] **Step 2: Add the field to the TypeScript schema**

In `src/Umbraco.Web.UI.Client/src/json-schema/umbraco-package-schema.ts`, add immediately after the `allowPublicAccess?: boolean;` block:

```typescript
	/**
	 * @title Decides if Umbraco automatically appends a per-package cache-busting token to this package's /App_Plugins importmap assets
	 * @default false
	 */
	disableCacheBusting?: boolean;
```

- [ ] **Step 3: Build Core to verify it compiles**

Run: `dotnet build src/Umbraco.Core`
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Core/Manifest/PackageManifest.cs src/Umbraco.Web.UI.Client/src/json-schema/umbraco-package-schema.ts
git commit -m "feat(core): add disableCacheBusting to package manifest model and schema"
```

---

### Task 2: Core helper `PackageManifestCacheBuster`

**Files:**
- Create: `src/Umbraco.Core/Manifest/PackageManifestCacheBuster.cs`
- Test: `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Manifest/PackageManifestCacheBusterTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/Umbraco.Tests.UnitTests/Umbraco.Core/Manifest/PackageManifestCacheBusterTests.cs`:

```csharp
using NUnit.Framework;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class PackageManifestCacheBusterTests
{
    private const string GlobalHash = "globalhash";

    [Test]
    public void ResolvePackageCacheBustHash_UsesVersionHash_WhenVersionPresent()
    {
        var result = PackageManifestCacheBuster.ResolvePackageCacheBustHash("1.2.3", GlobalHash);
        Assert.That(result, Is.EqualTo("1.2.3".GenerateHash()));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ResolvePackageCacheBustHash_FallsBackToGlobal_WhenVersionMissing(string? version)
    {
        var result = PackageManifestCacheBuster.ResolvePackageCacheBustHash(version, GlobalHash);
        Assert.That(result, Is.EqualTo(GlobalHash));
    }

    [Test]
    public void ApplyCacheBust_StampsAppPluginsPath()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js", "abc");
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=abc"));
    }

    [Test]
    public void ApplyCacheBust_IsCaseInsensitiveOnAppPluginsRoot()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/app_plugins/MyPkg/index.js", "abc");
        Assert.That(result, Is.EqualTo("/app_plugins/MyPkg/index.js?umb__rnd=abc"));
    }

    [Test]
    public void ApplyCacheBust_InsertsBeforeFragment()
    {
        var result = PackageManifestCacheBuster.ApplyCacheBust("/App_Plugins/MyPkg/index.js#frag", "abc");
        Assert.That(result, Is.EqualTo("/App_Plugins/MyPkg/index.js?umb__rnd=abc#frag"));
    }

    [TestCase("/umbraco/backoffice/apps/app/index.js")]         // backoffice core path
    [TestCase("@umbraco-cms/backoffice/router")]                // bare specifier
    [TestCase("https://cdn.example.com/pkg/index.js")]          // absolute CDN
    [TestCase("//cdn.example.com/pkg/index.js")]                // protocol-relative CDN
    [TestCase("./relative/index.js")]                           // relative (not App_Plugins-rooted)
    public void ApplyCacheBust_LeavesNonAppPluginsPathsUnchanged(string url)
    {
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenQueryAlreadyPresent()
    {
        const string url = "/App_Plugins/MyPkg/index.js?v=1";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }

    [Test]
    public void ApplyCacheBust_SkipsWhenCacheBusterTokenPresent()
    {
        const string url = "/App_Plugins/MyPkg/index.js?v=%CACHE_BUSTER%";
        Assert.That(PackageManifestCacheBuster.ApplyCacheBust(url, "abc"), Is.EqualTo(url));
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~PackageManifestCacheBusterTests"`
Expected: FAIL — `PackageManifestCacheBuster` does not exist (compile error).

- [ ] **Step 3: Implement the helper**

Create `src/Umbraco.Core/Manifest/PackageManifestCacheBuster.cs`:

```csharp
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Computes and applies per-package cache-busting tokens for package manifest assets.
/// </summary>
public static class PackageManifestCacheBuster
{
    private const string QueryParameterName = "umb__rnd";

    /// <summary>
    ///     Returns the cache-bust hash for a package: a hash of its <paramref name="packageVersion"/> when present,
    ///     otherwise the supplied global fallback hash.
    /// </summary>
    public static string ResolvePackageCacheBustHash(string? packageVersion, string fallbackHash)
        => string.IsNullOrWhiteSpace(packageVersion)
            ? fallbackHash
            : packageVersion.GenerateHash();

    /// <summary>
    ///     Computes Umbraco's global cache-bust hash (mirrors <c>UrlHelperExtensions.GetCacheBustHash</c>): a
    ///     restart-varying hash in debug mode, otherwise a hash of the Umbraco semantic version.
    /// </summary>
    public static string GetGlobalCacheBustHash(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion)
        => hostingEnvironment.IsDebugMode
            ? DateTime.Now.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture).GenerateHash()
            : umbracoVersion.SemanticVersion.ToSemanticString().GenerateHash();

    /// <summary>
    ///     Appends <c>?umb__rnd=&lt;hash&gt;</c> to a URL when, and only when, it is a clean <c>/App_Plugins</c>-rooted
    ///     path. URLs that carry the <c>%CACHE_BUSTER%</c> token, already have a query string, or point anywhere other
    ///     than <c>/App_Plugins</c> (backoffice core, CDN, bare module specifiers, relative paths) are returned unchanged.
    /// </summary>
    public static string ApplyCacheBust(string url, string hash)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        // %CACHE_BUSTER% is the explicit opt-in token, resolved elsewhere to the global hash — never auto-stamp it.
        if (url.Contains(Constants.Web.CacheBusterToken, StringComparison.Ordinal))
        {
            return url;
        }

        // Only ever touch the package's own /App_Plugins assets. This excludes the backoffice core
        // (/umbraco/backoffice/...), CDNs, protocol-relative URLs, bare specifiers and relative paths.
        if (url.StartsWith(Constants.SystemDirectories.AppPlugins, StringComparison.OrdinalIgnoreCase) is false)
        {
            return url;
        }

        // The author already manages this URL's query — leave it alone.
        if (url.Contains('?', StringComparison.Ordinal))
        {
            return url;
        }

        var fragmentIndex = url.IndexOf('#', StringComparison.Ordinal);
        return fragmentIndex < 0
            ? $"{url}?{QueryParameterName}={hash}"
            : $"{url[..fragmentIndex]}?{QueryParameterName}={hash}{url[fragmentIndex..]}";
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~PackageManifestCacheBusterTests"`
Expected: PASS (all cases).

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Core/Manifest/PackageManifestCacheBuster.cs tests/Umbraco.Tests.UnitTests/Umbraco.Core/Manifest/PackageManifestCacheBusterTests.cs
git commit -m "feat(core): add PackageManifestCacheBuster helper for per-package asset cache-busting"
```

---

### Task 3: Stamp the importmap per-package in `PackageManifestService`

**Files:**
- Modify: `src/Umbraco.Infrastructure/Manifest/PackageManifestService.cs`
- Test: `tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Manifest/PackageManifestServiceTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Manifest/PackageManifestServiceTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Manifest;

[TestFixture]
public class PackageManifestServiceTests
{
    private static PackageManifestService CreateService(params PackageManifest[] manifests)
    {
        var reader = new Mock<IPackageManifestReader>();
        reader.Setup(x => x.ReadPackageManifestsAsync()).ReturnsAsync(manifests);

        var runtimeSettings = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeSettings.Setup(x => x.CurrentValue).Returns(new RuntimeSettings { Mode = RuntimeMode.Production });

        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.Setup(x => x.IsDebugMode).Returns(false);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        return new PackageManifestService(
            new[] { reader.Object },
            AppCaches.Disabled,
            runtimeSettings.Object,
            hostingEnvironment.Object,
            umbracoVersion.Object);
    }

    private static PackageManifest Manifest(string name, string? version, bool disableCacheBusting, Dictionary<string, string> imports)
        => new()
        {
            Name = name,
            Version = version,
            DisableCacheBusting = disableCacheBusting,
            Extensions = Array.Empty<object>(),
            Importmap = new PackageManifestImportmap { Imports = imports },
        };

    [Test]
    public async Task GetPackageManifestImportmapAsync_StampsVersionHashOnAppPluginsImport()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            disableCacheBusting: false,
            new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo($"/App_Plugins/Pkg/index.js?umb__rnd={"2.0.0".GenerateHash()}"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_DoesNotStampWhenDisabled()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            disableCacheBusting: true,
            new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo("/App_Plugins/Pkg/index.js"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_FallsBackToGlobalHashWhenNoVersion()
    {
        var service = CreateService(Manifest(
            "Pkg",
            version: null,
            disableCacheBusting: false,
            new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        var expectedGlobal = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();
        Assert.That(result.Imports["pkg"], Is.EqualTo($"/App_Plugins/Pkg/index.js?umb__rnd={expectedGlobal}"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_LeavesNonAppPluginsAndBareSpecifiersUnchanged()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            disableCacheBusting: false,
            new Dictionary<string, string>
            {
                ["bare"] = "@scope/pkg",
                ["cdn"] = "https://cdn.example.com/x.js",
            }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["bare"], Is.EqualTo("@scope/pkg"));
        Assert.That(result.Imports["cdn"], Is.EqualTo("https://cdn.example.com/x.js"));
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~PackageManifestServiceTests"`
Expected: FAIL — the `PackageManifestService` constructor does not yet accept `IHostingEnvironment`/`IUmbracoVersion` (compile error).

- [ ] **Step 3: Update the constructor and importmap method**

In `src/Umbraco.Infrastructure/Manifest/PackageManifestService.cs`:

Add usings at the top:

```csharp
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
```

Add two readonly fields next to the existing ones:

```csharp
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IUmbracoVersion _umbracoVersion;
```

Replace the constructor signature and body assignments (it is `internal`, so no obsolete-constructor dance is needed; DI resolves the new parameters automatically because it is registered by type):

```csharp
    public PackageManifestService(
        IEnumerable<IPackageManifestReader> packageManifestReaders,
        AppCaches appCaches,
        IOptionsMonitor<RuntimeSettings> runtimeSettingsOptionsMonitor,
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion)
    {
        _packageManifestReaders = packageManifestReaders;
        _cache = appCaches.RuntimeCache;
        _runtimeSettings = runtimeSettingsOptionsMonitor.CurrentValue;
        runtimeSettingsOptionsMonitor.OnChange(runtimeSettings => _runtimeSettings = runtimeSettings);
        _hostingEnvironment = hostingEnvironment;
        _umbracoVersion = umbracoVersion;
    }
```

Replace the body of `GetPackageManifestImportmapAsync` with a per-manifest loop that stamps before merging (note: this builds fresh dictionaries, so the cached manifest instances are never mutated):

```csharp
    public async Task<PackageManifestImportmap> GetPackageManifestImportmapAsync()
    {
        IEnumerable<PackageManifest> packageManifests = await GetAllPackageManifestsAsync();
        var globalHash = PackageManifestCacheBuster.GetGlobalCacheBustHash(_hostingEnvironment, _umbracoVersion);

        var importDict = new Dictionary<string, string>();
        var scopesDict = new Dictionary<string, Dictionary<string, string>>();

        foreach (PackageManifest manifest in packageManifests)
        {
            PackageManifestImportmap? importmap = manifest.Importmap;
            if (importmap is null)
            {
                continue;
            }

            var stamp = manifest.DisableCacheBusting is false;
            var hash = stamp
                ? PackageManifestCacheBuster.ResolvePackageCacheBustHash(manifest.Version, globalHash)
                : string.Empty;

            foreach ((var key, var value) in importmap.Imports)
            {
                importDict[key] = stamp ? PackageManifestCacheBuster.ApplyCacheBust(value, hash) : value;
            }

            if (importmap.Scopes is null)
            {
                continue;
            }

            foreach ((var scopeKey, Dictionary<string, string> scopeImports) in importmap.Scopes)
            {
                var stampedScope = new Dictionary<string, string>();
                foreach ((var key, var value) in scopeImports)
                {
                    stampedScope[key] = stamp ? PackageManifestCacheBuster.ApplyCacheBust(value, hash) : value;
                }

                scopesDict[scopeKey] = stampedScope;
            }
        }

        return new PackageManifestImportmap
        {
            Imports = importDict,
            Scopes = scopesDict,
        };
    }
```

Add the using for the helper if not already present:

```csharp
using Umbraco.Cms.Core.Manifest;
```

(Note: the original used `ToDictionary`, which throws on duplicate keys across packages; the indexer-based merge here is last-wins, which is more forgiving and preserves the previous "all imports combined" intent.)

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~PackageManifestServiceTests"`
Expected: PASS (all cases).

- [ ] **Step 5: Build the Infrastructure project to confirm DI wiring still compiles**

Run: `dotnet build src/Umbraco.Infrastructure`
Expected: Build succeeded, 0 errors. (DI registration in `UmbracoBuilder.CoreServices.cs` is `AddSingleton<IPackageManifestService, PackageManifestService>()` — type-based, so the new constructor parameters resolve automatically.)

- [ ] **Step 6: Commit**

```bash
git add src/Umbraco.Infrastructure/Manifest/PackageManifestService.cs tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/Manifest/PackageManifestServiceTests.cs
git commit -m "feat(core): auto-append per-package cache-buster to /App_Plugins importmap assets (closes #68840)"
```

---

### Task 4: Verify no regression to the backoffice importmap (manual confirmation)

**Files:** none (verification only).

- [ ] **Step 1: Confirm the backoffice core importmap is untouched**

The backoffice's own `umbraco-package.json` (`src/Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice/umbraco-package.json`) has importmap entries rooted at `/umbraco/backoffice/...`. `ApplyCacheBust` only stamps `/App_Plugins`-rooted paths, so these are returned unchanged and continue to be busted by the existing `/umbraco/backoffice` → `/umbraco/backoffice/<hash>` replace in `BackOfficeImportMapScriptAsync`.

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~PackageManifestCacheBusterTests.ApplyCacheBust_LeavesNonAppPluginsPathsUnchanged"`
Expected: PASS — the `/umbraco/backoffice/apps/app/index.js` case proves backoffice paths are not stamped.

- [ ] **Step 2: Full unit-test sweep for the manifest area**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~Manifest"`
Expected: PASS — no existing manifest tests regress.

---

## Out of scope (sibling tasks under PBI #68839)

- **`Cache-Control` header defaults** for `/App_Plugins` static responses (the headers that make the busting worthwhile).
- **Bundling guidance** docs.
- **CDN caveat doc:** query-string busting only works where the CDN/proxy includes the query string in its cache key (CloudFront and many Azure Front Door / reverse-proxy configs ignore it by default → silent stale). This warning MUST ship with the headers/bundling guidance.
- **Extension asset-URL auto-stamping** (`js`/`element`/`api`/`css` fields in `Extensions`) — deferred as fragile (untyped JSON traversal across all extension kinds); authors can use `%CACHE_BUSTER%` for those today.

---

## Self-Review

- **Spec coverage:** #68840 ("hash based on their version or other good default") → Task 2 `ResolvePackageCacheBustHash` (version → hash, else global fallback). "Other good default" → global Umbraco hash fallback. Auto-application → Task 3. Opt-out → Task 1 `disableCacheBusting`. ✅
- **Placeholder scan:** no TBD/“handle errors”/“similar to”; every code step is complete. ✅
- **Type consistency:** `ResolvePackageCacheBustHash(string?, string)`, `GetGlobalCacheBustHash(IHostingEnvironment, IUmbracoVersion)`, `ApplyCacheBust(string, string)`, `DisableCacheBusting` (bool) used identically across helper, service, and tests. Query param `umb__rnd` matches the existing convention (`UrlHelperExtensions.GetUrlWithCacheBust`). ✅
- **Regression guard:** auto-stamp restricted to `/App_Plugins` and skips `%CACHE_BUSTER%`-bearing URLs → backoffice core (`/umbraco/backoffice`, no token, clean paths) is provably untouched (Task 4). ✅
- **Risk to verify during execution:** confirm `SemVersion`/`ToSemanticString()` and `AppCaches.Disabled` are the correct symbols in the test project (mirror an existing Infrastructure unit test's usings if the build complains).
```
