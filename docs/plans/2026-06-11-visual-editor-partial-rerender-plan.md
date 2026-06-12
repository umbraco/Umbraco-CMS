# Visual Editor — Partial Re-render (Phase 3 remainder) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make server-side partial re-render the single mechanism for reflecting edits in the visual editor preview — render the page with the workspace's unsaved values and morph the live iframe DOM in place, with no reload.

**Architecture:** A new public `IVisualEditorContentFactory` (implemented in `Umbraco.PublishedCache.HybridCache`, where the required cache factories live) builds an `IPublishedContent` whose overridden properties carry unsaved editor values — the conversion path proven by the spike at `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/Spikes/PartialRerenderConversionSpikeTests.cs`. A new `IVisualEditorRenderService` in `Umbraco.Web.Common` (modeled on `TemplateRenderer`) renders that content to an HTML string with preview-mode property-tracking enabled. A `POST /umbraco/management/api/v1/visual-editor/render` endpoint exposes it. On the client, a new `UmbVisualEditorRenderController` (sibling to the existing SignalR/router/resolver controllers) debounces edits and posts `umb:ve:render` to the guest, which bundles **morphdom**, re-runs a now-callable `initRegions()`, and restores selection.

**Tech Stack:** C# / ASP.NET Core (server), TypeScript + Lit (backoffice client), esbuild IIFE bundle (guest script). Working directory for ALL tasks: `D:/CMS/Umbraco-CMS/.worktrees/feature-visual-editor`.

**Design source:** `docs/plans/2026-06-11-visual-editor-partial-rerender-design.md` (spike gate PASSED 2026-06-11).

**Deliberate deviation from the design (justified):** The design named a `PropertyOverridePublishedContent` *decorator*. We instead reuse the **cache-node override + `IPublishedContentFactory.ToIPublishedContent`** path that the spike proved end-to-end. Reasons: (1) it is already proven; (2) it avoids hand-reimplementing the full `IPublishedProperty` conversion chain (`ConvertSourceToInter`/`ConvertInterToObject`, variation contextualisation, caching, delivery-api value) which a decorator's override property would have to duplicate; (3) `ICacheNodeFactory`/`IPublishedContentFactory` are `internal` to HybridCache (verified), so the builder must live there anyway. Same observable contract as the design: an `IPublishedContent` whose overridden aliases return converted unsaved values, everything else identical to the draft.

**Testing note:** Backend conversion (the riskiest part) gets a real integration test (Task 1). The render service gets a runnable integration test using a captured fake view engine (Task 2) — the integration host does not compile/serve physical Razor views, so full render fidelity is validated by the manual smoke (Task 8), consistent with this feature's established posture (frontend = build + lint + manual). This is a spec-aligned scoping of "the backend integration test".

**Verified facts this plan relies on** (checked against source 2026-06-11):
- `ICacheNodeFactory` (`src/Umbraco.PublishedCache.HybridCache/Factories/ICacheNodeFactory.cs:5`) and `IPublishedContentFactory` (`.../IPublishedContentFactory.cs:9`) are **`internal`** to HybridCache. The override builder must live in that project.
- `Umbraco.Web.Common` already references `Umbraco.PublishedCache.HybridCache` (`Umbraco.Web.Common.csproj:37`), so its render service can consume a **public** interface exposed from HybridCache (the interface itself lives in `Umbraco.Core`, which both reference).
- `IContentService` has `GetById(int id)` (`IContentService.cs:93`) but **no `GetById(Guid)`** — resolve the int id via `IIdKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document)` first (the spike uses `IIdKeyMap` the same way for data types).
- `UmbracoViewPage.TryWriteVisualEditorAnnotation` gates on `(UmbracoContext?.InPreviewMode ?? false)` at `src/Umbraco.Web.Common/Views/UmbracoViewPage.cs:147`. In an API render scope `InPreviewMode` is false (it's a backoffice request), so this clause must be relaxed to also accept `VisualEditorPropertyTracker.IsEnabled` (Task 2) — otherwise no `data-umb-property` spans are emitted.
- `VisualEditorPropertyTracker` (`src/Umbraco.Core/Models/PublishedContent/VisualEditorPropertyTracker.cs`) has `Enable()`/`IsEnabled` but **no `Disable()`** — add one (Task 2) so the render scope can reset it in a `finally`.
- `FromEditor` reads `editorValue.Value` as a **serialized JSON string** for complex editors (RTE/Block List); plain TextBox is a raw string. The builder serializes non-string override values (spike proved this exact fix).
- The guest `injected.ts` runs all per-node setup **inline** in one IIFE; document-level `click`/`mouseover`/`message` listeners survive a DOM morph, but per-node outlines/action-bars/drag-handlers/add-buttons do not and must be re-applied by `initRegions()`.

---

### Task 1: `IVisualEditorContentFactory` — build IPublishedContent with unsaved overrides

**Files:**
- Create: `src/Umbraco.Core/PublishedCache/IVisualEditorContentFactory.cs`
- Create: `src/Umbraco.Core/PublishedCache/VisualEditorPropertyOverride.cs`
- Create: `src/Umbraco.PublishedCache.HybridCache/Services/VisualEditorContentFactory.cs`
- Modify: HybridCache DI registration (located in Step 5)
- Test: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/PublishedCache/VisualEditorContentFactoryTests.cs`

- [ ] **Step 1: Define the public contract (Core)**

Create `src/Umbraco.Core/PublishedCache/VisualEditorPropertyOverride.cs`:

```csharp
namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// A single unsaved property value to overlay onto draft content when rendering the visual editor preview.
/// </summary>
/// <param name="Alias">The property alias to override.</param>
/// <param name="EditorValue">
/// The editor-format value as held by the backoffice workspace. Complex editors (rich text, block list)
/// expect their serialized JSON; plain editors (e.g. text box) expect the raw value.
/// </param>
/// <param name="Culture">The culture the override applies to, or <c>null</c> for invariant.</param>
/// <param name="Segment">The segment the override applies to, or <c>null</c> for none.</param>
public readonly record struct VisualEditorPropertyOverride(string Alias, object? EditorValue, string? Culture, string? Segment);
```

Create `src/Umbraco.Core/PublishedCache/IVisualEditorContentFactory.cs`:

```csharp
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Builds an <see cref="IPublishedContent"/> for the visual editor preview: the requested document's
/// draft content with a set of unsaved property values overlaid on top, converted to their published form.
/// </summary>
public interface IVisualEditorContentFactory
{
    /// <summary>
    /// Resolves the draft content for <paramref name="documentKey"/> and returns a preview
    /// <see cref="IPublishedContent"/> whose overridden aliases yield the converted unsaved values.
    /// Returns <c>null</c> if the document does not exist.
    /// </summary>
    Task<IPublishedContent?> CreateWithOverridesAsync(
        Guid documentKey,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides);
}
```

- [ ] **Step 2: Write the failing integration test**

Create `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/PublishedCache/VisualEditorContentFactoryTests.cs`. This is the spike formalised against the production interface:

```csharp
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PublishedCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class VisualEditorContentFactoryTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IVisualEditorContentFactory VisualEditorContentFactory => GetRequiredService<IVisualEditorContentFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public async Task CreateWithOverrides_Returns_Converted_Unsaved_Values()
    {
        var elementType = ContentTypeBuilder.CreateAllTypesContentType("spikeElement", "Spike Element");
        elementType.IsElement = true;
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var blockListDataType = new DataType(PropertyEditorCollection[Constants.PropertyEditors.Aliases.BlockList], ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    "blocks",
                    new BlockListConfiguration.BlockConfiguration[] { new() { ContentElementTypeKey = elementType.Key } }
                }
            },
            Name = "Spike Block List",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };
        await DataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("spikePage")
            .WithName("Spike Page")
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("title").WithName("Title").Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                    .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .WithAlias("rte").WithName("RTE").Done()
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                    .WithDataTypeId(blockListDataType.Id)
                    .WithValueStorageType(ValueStorageType.Ntext)
                    .WithAlias("blocks").WithName("Blocks").Done()
                .Done()
            .Build();
        Assert.IsTrue((await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey)).Success);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Spike Doc")
            .WithPropertyValues(new { title = "Original title" })
            .Build();
        Assert.IsTrue(ContentService.Save(content).Success);
        Assert.IsTrue(ContentService.Publish(content, []).Success);

        var blockContentKey = Guid.NewGuid();
        var blockListEditorValue = new BlockListValue
        {
            Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
            {
                { Constants.PropertyEditors.Aliases.BlockList, new IBlockLayoutItem[] { new BlockListLayoutItem { ContentKey = blockContentKey } } }
            },
            ContentData =
            [
                new()
                {
                    Key = blockContentKey,
                    ContentTypeAlias = elementType.Alias,
                    ContentTypeKey = elementType.Key,
                    Values = [ new() { Alias = "singleLineText", Value = "Block text value" } ]
                }
            ],
            Expose = [ new(blockContentKey, null, null) ]
        };

        var overrides = new[]
        {
            new VisualEditorPropertyOverride("title", "Overridden title", null, null),
            new VisualEditorPropertyOverride("rte", new RichTextEditorValue { Markup = "<p>Overridden rich text</p>", Blocks = null }, null, null),
            new VisualEditorPropertyOverride("blocks", blockListEditorValue, null, null),
        };

        IPublishedContent? result = await VisualEditorContentFactory.CreateWithOverridesAsync(content.Key, overrides);

        Assert.IsNotNull(result);
        Assert.AreEqual("Overridden title", result!.Value("title"));
        StringAssert.Contains("Overridden rich text", result.Value("rte")!.ToString());

        var blockListModel = result.Value("blocks") as BlockListModel;
        Assert.IsNotNull(blockListModel, "Block List should convert to a BlockListModel");
        Assert.AreEqual(1, blockListModel!.Count);
        Assert.AreEqual("Block text value", blockListModel.First().Content.Value("singleLineText"));
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~VisualEditorContentFactoryTests"`
Expected: FAIL — `IVisualEditorContentFactory` not registered / not implemented (build error or DI resolution failure).

- [ ] **Step 4: Implement the factory (HybridCache)**

Create `src/Umbraco.PublishedCache.HybridCache/Services/VisualEditorContentFactory.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class VisualEditorContentFactory : IVisualEditorContentFactory
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IContentService _contentService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IJsonSerializer _jsonSerializer;

    public VisualEditorContentFactory(
        IIdKeyMap idKeyMap,
        IContentService contentService,
        IDataTypeService dataTypeService,
        ICacheNodeFactory cacheNodeFactory,
        IPublishedContentFactory publishedContentFactory,
        IJsonSerializer jsonSerializer)
    {
        _idKeyMap = idKeyMap;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _cacheNodeFactory = cacheNodeFactory;
        _publishedContentFactory = publishedContentFactory;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<IPublishedContent?> CreateWithOverridesAsync(
        Guid documentKey,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(documentKey, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return null;
        }

        IContent? content = _contentService.GetById(idAttempt.Result);
        if (content is null)
        {
            return null;
        }

        ContentCacheNode baseNode = _cacheNodeFactory.ToContentCacheNode(content, preview: true);
        if (baseNode.Data is null)
        {
            return null;
        }

        var properties = new Dictionary<string, PropertyData[]>(baseNode.Data.Properties);

        foreach (VisualEditorPropertyOverride @override in overrides)
        {
            object? source = await ConvertToSourceValueAsync(content, @override);
            properties[@override.Alias] =
            [
                new PropertyData
                {
                    Culture = @override.Culture ?? string.Empty,
                    Segment = @override.Segment ?? string.Empty,
                    Value = source,
                }
            ];
        }

#pragma warning disable CS0618 // ContentData ctor obsolete usage mirrored from the cache node source
        var overriddenNode = new ContentCacheNode
        {
            Id = baseNode.Id,
            Key = baseNode.Key,
            SortOrder = baseNode.SortOrder,
            CreateDate = baseNode.CreateDate,
            CreatorId = baseNode.CreatorId,
            ContentTypeId = baseNode.ContentTypeId,
            IsDraft = true,
            Data = new ContentData(
                name: baseNode.Data.Name,
                urlSegment: baseNode.Data.UrlSegment,
                versionId: baseNode.Data.VersionId,
                versionDate: baseNode.Data.VersionDate,
                writerId: baseNode.Data.WriterId,
                templateId: baseNode.Data.TemplateId,
                published: baseNode.Data.Published,
                properties: properties,
                cultureInfos: baseNode.Data.CultureInfos),
        };
#pragma warning restore CS0618

        return _publishedContentFactory.ToIPublishedContent(overriddenNode, preview: true);
    }

    private async Task<object?> ConvertToSourceValueAsync(IContent content, VisualEditorPropertyOverride @override)
    {
        IProperty? property = content.Properties[@override.Alias];
        if (property is null)
        {
            return null;
        }

        IDataType? dataType = await _dataTypeService.GetAsync(property.PropertyType.DataTypeKey);
        if (dataType?.Editor is null)
        {
            return null;
        }

        // FromEditor expects the serialized editor value for complex editors; a plain string passes through.
        var editorValue = @override.EditorValue is string s ? s : _jsonSerializer.Serialize(@override.EditorValue);

        return dataType.Editor.GetValueEditor().FromEditor(
            new ContentPropertyData(editorValue, dataType.ConfigurationObject),
            null);
    }
}
```

- [ ] **Step 5: Register the service (HybridCache)**

Find where HybridCache registers its internal factories:

Run: `grep -rn "ICacheNodeFactory\|AddSingleton<IPublishedContentFactory" src/Umbraco.PublishedCache.HybridCache/DependencyInjection/`

Open the file that registers `ICacheNodeFactory` (a `UmbracoBuilder` extension). Add this line alongside the other factory registrations:

```csharp
builder.Services.AddSingleton<IVisualEditorContentFactory, VisualEditorContentFactory>();
```

Add the using if needed: `using Umbraco.Cms.Core.PublishedCache;` and `using Umbraco.Cms.Infrastructure.HybridCache.Services;`.

- [ ] **Step 6: Run the test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~VisualEditorContentFactoryTests"`
Expected: PASS (1 passed).

- [ ] **Step 7: Commit**

```bash
git add src/Umbraco.Core/PublishedCache/IVisualEditorContentFactory.cs src/Umbraco.Core/PublishedCache/VisualEditorPropertyOverride.cs src/Umbraco.PublishedCache.HybridCache/ tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/PublishedCache/VisualEditorContentFactoryTests.cs
git commit -m "feat(visual-editor): build IPublishedContent with unsaved value overrides for preview render"
```

---

### Task 2: `IVisualEditorRenderService` — render overridden content to HTML

**Files:**
- Modify: `src/Umbraco.Core/Models/PublishedContent/VisualEditorPropertyTracker.cs` (add `Disable()`)
- Modify: `src/Umbraco.Web.Common/Views/UmbracoViewPage.cs:147` (relax annotation gate)
- Create: `src/Umbraco.Core/Templates/IVisualEditorRenderService.cs`
- Create: `src/Umbraco.Web.Common/Templates/VisualEditorRenderService.cs`
- Modify: Web.Common DI registration (located in Step 6)
- Test: `tests/Umbraco.Tests.Integration/Umbraco.Web.Common/VisualEditorRenderServiceTests.cs`

- [ ] **Step 1: Add `Disable()` to the tracker**

In `src/Umbraco.Core/Models/PublishedContent/VisualEditorPropertyTracker.cs`, directly after the `Enable()` method add:

```csharp
    /// <summary>
    /// Disables tracking for the current async context. Pair with <see cref="Enable"/> in a finally block.
    /// </summary>
    public static void Disable() => _enabled.Value = false;
```

- [ ] **Step 2: Relax the annotation gate so it works in the render scope**

In `src/Umbraco.Web.Common/Views/UmbracoViewPage.cs`, in `TryWriteVisualEditorAnnotation` (line 147) replace:

```csharp
            && (UmbracoContext?.InPreviewMode ?? false))
```

with:

```csharp
            && ((UmbracoContext?.InPreviewMode ?? false) || VisualEditorPropertyTracker.IsEnabled))
```

(The `propertyAccess.HasValue` guard already ensures the tracker recorded an editable-in-visual-editor property, so honouring `IsEnabled` here is the correct visual-editor signal when rendering outside a cookie-driven preview request. Add `using Umbraco.Cms.Core.Models.PublishedContent;` to the file if it is not already present.)

- [ ] **Step 3: Define the render service contract (Core)**

Create `src/Umbraco.Core/Templates/IVisualEditorRenderService.cs`:

```csharp
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
/// Renders a document's assigned template to an HTML string using the visual editor's unsaved values,
/// with property-access tracking enabled so the output carries <c>data-umb-*</c> annotations.
/// </summary>
public interface IVisualEditorRenderService
{
    /// <summary>
    /// Renders the document identified by <paramref name="documentKey"/> with the supplied unsaved
    /// <paramref name="overrides"/> overlaid. Returns the rendered HTML, or an empty string if the
    /// document or its template cannot be resolved.
    /// </summary>
    Task<string> RenderAsync(
        Guid documentKey,
        string? culture,
        string? segment,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides);
}
```

- [ ] **Step 4: Write the failing integration test (captured fake view engine)**

Create `tests/Umbraco.Tests.Integration/Umbraco.Web.Common/VisualEditorRenderServiceTests.cs`. The fake view engine captures the model and tracker state at render time, so the test runs without physical Razor views:

```csharp
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Common;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class VisualEditorRenderServiceTests : UmbracoIntegrationTest
{
    private static readonly CapturingViewEngine Captured = new();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IVisualEditorRenderService RenderService => GetRequiredService<IVisualEditorRenderService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.AddUnique<ICompositeViewEngine>(Captured);
    }

    [Test]
    public async Task RenderAsync_Passes_Overridden_Content_With_Tracking_Enabled()
    {
        ITemplate template = TemplateBuilder.CreateTextPageTemplate("spikeTemplate");
        await FileService.CreateTemplateAsync(template, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("spikePage")
            .WithName("Spike Page")
            .WithDefaultTemplateId(template.Id)
            .AddAllowedTemplate().WithId(template.Id).WithAlias(template.Alias).Done()
            .AddPropertyGroup().WithName("Content").WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("title").WithName("Title").Done()
                .Done()
            .Build();
        Assert.IsTrue((await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey)).Success);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Spike Doc")
            .WithTemplateId(template.Id)
            .WithPropertyValues(new { title = "Original title" })
            .Build();
        Assert.IsTrue(ContentService.Save(content).Success);
        Assert.IsTrue(ContentService.Publish(content, []).Success);

        var overrides = new[] { new VisualEditorPropertyOverride("title", "Overridden title", null, null) };

        await RenderService.RenderAsync(content.Key, null, null, overrides);

        Assert.IsNotNull(Captured.LastModel, "The render service did not pass a model to the view engine.");
        var model = Captured.LastModel as IPublishedContent;
        Assert.IsNotNull(model);
        Assert.AreEqual("Overridden title", model!.Value("title"));
        Assert.IsTrue(Captured.TrackerEnabledDuringRender, "VisualEditorPropertyTracker must be enabled while rendering.");
    }

    private sealed class CapturingViewEngine : ICompositeViewEngine
    {
        public object? LastModel { get; private set; }

        public bool TrackerEnabledDuringRender { get; private set; }

        public ViewEngineResult FindView(Microsoft.AspNetCore.Mvc.ActionContext context, string viewName, bool isMainPage)
            => ViewEngineResult.Found(viewName, new CapturingView(this));

        public ViewEngineResult GetView(string? executingFilePath, string viewPath, bool isMainPage)
            => ViewEngineResult.Found(viewPath, new CapturingView(this));

        private void Capture(ViewContext viewContext)
        {
            LastModel = viewContext.ViewData.Model;
            TrackerEnabledDuringRender = VisualEditorPropertyTracker.IsEnabled;
        }

        private sealed class CapturingView : IView
        {
            private readonly CapturingViewEngine _owner;

            public CapturingView(CapturingViewEngine owner) => _owner = owner;

            public string Path => "captured";

            public Task RenderAsync(ViewContext context)
            {
                _owner.Capture(context);
                return Task.CompletedTask;
            }
        }
    }
}
```

(If `TemplateBuilder`/`AddAllowedTemplate`/`WithDefaultTemplateId`/`WithTemplateId` builder helpers differ in the test project, adjust to the available builder API — the only requirements are: a saved template, a content type whose default template is that template, and a published document on it.)

- [ ] **Step 5: Run the test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~VisualEditorRenderServiceTests"`
Expected: FAIL — `IVisualEditorRenderService` not registered.

- [ ] **Step 6: Implement the render service (Web.Common)**

Create `src/Umbraco.Web.Common/Templates/VisualEditorRenderService.cs`. This mirrors `TemplateRenderer` (`src/Umbraco.Web.Common/Templates/TemplateRenderer.cs`) but sets the content directly and enables the tracker:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Templates;

internal sealed class VisualEditorRenderService : IVisualEditorRenderService
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IPublishedRouter _publishedRouter;
    private readonly ITemplateService _templateService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICompositeViewEngine _viewEngine;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IVisualEditorContentFactory _contentFactory;

    public VisualEditorRenderService(
        IUmbracoContextFactory umbracoContextFactory,
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishedRouter publishedRouter,
        ITemplateService templateService,
        IHttpContextAccessor httpContextAccessor,
        ICompositeViewEngine viewEngine,
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IVisualEditorContentFactory contentFactory)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _umbracoContextAccessor = umbracoContextAccessor;
        _publishedRouter = publishedRouter;
        _templateService = templateService;
        _httpContextAccessor = httpContextAccessor;
        _viewEngine = viewEngine;
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _contentFactory = contentFactory;
    }

    public async Task<string> RenderAsync(
        Guid documentKey,
        string? culture,
        string? segment,
        IReadOnlyCollection<VisualEditorPropertyOverride> overrides)
    {
        using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
        IUmbracoContext umbracoContext = contextReference.UmbracoContext;

        IPublishedContent? content = await _contentFactory.CreateWithOverridesAsync(documentKey, overrides);
        if (content?.TemplateId is null)
        {
            return string.Empty;
        }

        ITemplate? template = await _templateService.GetAsync(content.TemplateId.Value);
        if (template is null)
        {
            return string.Empty;
        }

        IPublishedRequestBuilder requestBuilder = await _publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
        requestBuilder.SetCulture(culture);
        requestBuilder.SetPublishedContent(content);
        requestBuilder.SetTemplate(template);
        IPublishedRequest request = requestBuilder.Build();

        IPublishedRequest? oldRequest = umbracoContext.PublishedRequest;
        VisualEditorPropertyTracker.Enable();
        try
        {
            umbracoContext.PublishedRequest = request;
            return ExecuteTemplateRendering(request);
        }
        finally
        {
            VisualEditorPropertyTracker.Disable();
            umbracoContext.PublishedRequest = oldRequest;
        }
    }

    private string ExecuteTemplateRendering(IPublishedRequest request)
    {
        HttpContext httpContext = _httpContextAccessor.GetRequiredHttpContext();

        ViewEngineResult viewResult = _viewEngine.GetView(null, $"~/Views/{request.GetTemplateAlias()}.cshtml", true);
        if (viewResult.View is null)
        {
            return string.Empty;
        }

        var viewData = new ViewDataDictionary(_modelMetadataProvider, new ModelStateDictionary())
        {
            Model = request.PublishedContent,
        };

        using var writer = new StringWriter();
        var viewContext = new ViewContext(
            new ActionContext(httpContext, httpContext.GetRouteData(), new ControllerActionDescriptor()),
            viewResult.View,
            viewData,
            _tempDataDictionaryFactory.GetTempData(httpContext),
            writer,
            new HtmlHelperOptions());

        viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
        return writer.GetStringBuilder().ToString();
    }
}
```

- [ ] **Step 7: Register the service (Web.Common)**

Find where `ITemplateRenderer` is registered:

Run: `grep -rn "ITemplateRenderer" src/Umbraco.Web.Common/DependencyInjection/ src/Umbraco.Core/DependencyInjection/`

In the same registration extension, alongside `ITemplateRenderer`, add:

```csharp
builder.Services.AddTransient<IVisualEditorRenderService, VisualEditorRenderService>();
```

Add usings: `using Umbraco.Cms.Core.Templates;` and `using Umbraco.Cms.Web.Common.Templates;`.

- [ ] **Step 8: Run the test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~VisualEditorRenderServiceTests"`
Expected: PASS.

- [ ] **Step 9: Build the affected projects**

Run: `dotnet build src/Umbraco.Web.Common/Umbraco.Web.Common.csproj`
Expected: Build succeeded, 0 errors.

- [ ] **Step 10: Commit**

```bash
git add src/Umbraco.Core/Models/PublishedContent/VisualEditorPropertyTracker.cs src/Umbraco.Web.Common/Views/UmbracoViewPage.cs src/Umbraco.Core/Templates/IVisualEditorRenderService.cs src/Umbraco.Web.Common/Templates/VisualEditorRenderService.cs src/Umbraco.Web.Common/DependencyInjection/ tests/Umbraco.Tests.Integration/Umbraco.Web.Common/VisualEditorRenderServiceTests.cs
git commit -m "feat(visual-editor): render service producing annotated HTML from unsaved values"
```

---

### Task 3: Management API endpoint

**Files:**
- Create: `src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/VisualEditorRenderRequestModel.cs`
- Create: `src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/VisualEditorPropertyValueModel.cs`
- Create: `src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/VisualEditorRenderResponseModel.cs`
- Create: `src/Umbraco.Cms.Api.Management/Controllers/VisualEditor/VisualEditorControllerBase.cs`
- Create: `src/Umbraco.Cms.Api.Management/Controllers/VisualEditor/RenderVisualEditorController.cs`

- [ ] **Step 1: Request/response models**

Create `src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/VisualEditorPropertyValueModel.cs`:

```csharp
namespace Umbraco.Cms.Api.Management.ViewModels.VisualEditor;

/// <summary>
/// A single unsaved property value submitted for a visual editor preview render.
/// </summary>
public class VisualEditorPropertyValueModel
{
    /// <summary>Gets or sets the property alias.</summary>
    public required string Alias { get; set; }

    /// <summary>Gets or sets the editor-format value (raw string for simple editors, JSON for complex editors).</summary>
    public object? Value { get; set; }

    /// <summary>Gets or sets the culture this value applies to, or <c>null</c> for invariant.</summary>
    public string? Culture { get; set; }

    /// <summary>Gets or sets the segment this value applies to, or <c>null</c> for none.</summary>
    public string? Segment { get; set; }
}
```

Create `src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/VisualEditorRenderRequestModel.cs`:

```csharp
namespace Umbraco.Cms.Api.Management.ViewModels.VisualEditor;

/// <summary>
/// Request to render a document's template with unsaved visual editor values overlaid.
/// </summary>
public class VisualEditorRenderRequestModel
{
    /// <summary>Gets or sets the document key to render.</summary>
    public Guid Unique { get; set; }

    /// <summary>Gets or sets the culture to render, or <c>null</c> for the default/invariant.</summary>
    public string? Culture { get; set; }

    /// <summary>Gets or sets the segment to render, or <c>null</c> for none.</summary>
    public string? Segment { get; set; }

    /// <summary>Gets or sets the unsaved property values to overlay onto the draft content.</summary>
    public IEnumerable<VisualEditorPropertyValueModel> Values { get; set; } = [];
}
```

Create `src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/VisualEditorRenderResponseModel.cs`:

```csharp
namespace Umbraco.Cms.Api.Management.ViewModels.VisualEditor;

/// <summary>
/// The rendered HTML for a visual editor preview render request.
/// </summary>
public class VisualEditorRenderResponseModel
{
    /// <summary>Gets or sets the rendered page HTML.</summary>
    public required string Html { get; set; }
}
```

- [ ] **Step 2: Controller base**

First confirm the common base controller name:

Run: `grep -rn "class ManagementApiControllerBase" src/Umbraco.Cms.Api.Management/`

Create `src/Umbraco.Cms.Api.Management/Controllers/VisualEditor/VisualEditorControllerBase.cs` (inherit the base reported above; it is `ManagementApiControllerBase`):

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.VisualEditor;

[VersionedApiBackOfficeRoute("visual-editor")]
[ApiExplorerSettings(GroupName = "Visual Editor")]
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public abstract class VisualEditorControllerBase : ManagementApiControllerBase
{
}
```

- [ ] **Step 3: Render controller**

Create `src/Umbraco.Cms.Api.Management/Controllers/VisualEditor/RenderVisualEditorController.cs`:

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.VisualEditor;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Api.Management.Controllers.VisualEditor;

/// <summary>
/// Renders a document's template with the visual editor's unsaved values for live preview.
/// </summary>
public class RenderVisualEditorController : VisualEditorControllerBase
{
    private readonly IVisualEditorRenderService _renderService;

    public RenderVisualEditorController(IVisualEditorRenderService renderService)
        => _renderService = renderService;

    [HttpPost("render")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(VisualEditorRenderResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Renders a document with unsaved visual editor values.")]
    public async Task<IActionResult> Render(
        CancellationToken cancellationToken,
        VisualEditorRenderRequestModel requestModel)
    {
        var overrides = requestModel.Values
            .Select(v => new VisualEditorPropertyOverride(v.Alias, v.Value, v.Culture, v.Segment))
            .ToList();

        var html = await _renderService.RenderAsync(
            requestModel.Unique,
            requestModel.Culture,
            requestModel.Segment,
            overrides);

        return Ok(new VisualEditorRenderResponseModel { Html = html });
    }
}
```

- [ ] **Step 4: Build the Management API**

Run: `dotnet build src/Umbraco.Cms.Api.Management/Umbraco.Cms.Api.Management.csproj`
Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Cms.Api.Management/ViewModels/VisualEditor/ src/Umbraco.Cms.Api.Management/Controllers/VisualEditor/
git commit -m "feat(visual-editor): add management API render endpoint"
```

---

### Task 4: Regenerate OpenApi.json + TypeScript client

**Files:**
- Modify: `src/Umbraco.Cms.Api.Management/OpenApi.json`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/core/backend-api/` (generated)

- [ ] **Step 1: Run the site and capture the updated OpenApi.json**

Per root `CLAUDE.md` → "Updating `OpenApi.json`":
1. Run: `dotnet run --project src/Umbraco.Web.UI` (or use an already-running instance).
2. Open the management swagger JSON (e.g. `https://localhost:44339/umbraco/swagger/management/swagger.json`).
3. Copy the full JSON and paste it into `src/Umbraco.Cms.Api.Management/OpenApi.json`.
4. Commit ONLY the substantive additions (the new `/visual-editor/render` path + the three new schemas) — discard IDE reformatting/whitespace/reordering noise via `git add -p`.

- [ ] **Step 2: Regenerate the TypeScript client**

Run:
```bash
cd src/Umbraco.Web.UI.Client
npm run generate:server-api
```

Confirm a `VisualEditorService` (from the `Visual Editor` group) with a `postVisualEditorRender` method now exists:

Run: `grep -rn "VisualEditorService\|postVisualEditorRender" src/packages/core/backend-api/`
Expected: the generated service + method appear. (If the generated method name differs, note the exact name — Task 5 uses it.)

- [ ] **Step 3: Build the client to confirm the generated types compile**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 4: Commit**

```bash
git add src/Umbraco.Cms.Api.Management/OpenApi.json src/Umbraco.Web.UI.Client/src/packages/core/backend-api/
git commit -m "chore(visual-editor): regenerate OpenApi.json and server API client for render endpoint"
```

---

### Task 5: Client render controller

**Files:**
- Create: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-render.controller.ts`

- [ ] **Step 1: Create the controller**

Create `visual-editor-render.controller.ts`. It follows the SignalR controller pattern (extends `UmbControllerBase`, callbacks in the constructor), debounces ~500ms, and uses an `AbortController` for latest-wins:

```typescript
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { VisualEditorService } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbVisualEditorRenderInput {
	unique: string;
	culture?: string;
	segment?: string;
	values: Array<{ alias: string; value: unknown; culture?: string; segment?: string }>;
}

const DEBOUNCE_MS = 500;

/**
 * Debounces visual editor edits and requests a server-side partial re-render, posting the resulting
 * HTML to the guest via the supplied callback. Latest-wins: a newer request aborts the in-flight one,
 * so stale HTML never overwrites newer DOM. On failure the last good DOM is kept (caller is notified).
 */
export class UmbVisualEditorRenderController extends UmbControllerBase {
	#getInput: () => UmbVisualEditorRenderInput | undefined;
	#postRender: (html: string) => void;
	#onError?: () => void;

	#timer?: ReturnType<typeof setTimeout>;
	#abort?: AbortController;

	constructor(
		host: UmbControllerHost,
		args: {
			getInput: () => UmbVisualEditorRenderInput | undefined;
			postRender: (html: string) => void;
			onError?: () => void;
		},
	) {
		super(host);
		this.#getInput = args.getInput;
		this.#postRender = args.postRender;
		this.#onError = args.onError;
	}

	/** Schedule a debounced re-render. Repeated calls within the debounce window collapse to one request. */
	requestRender() {
		if (this.#timer) clearTimeout(this.#timer);
		this.#timer = setTimeout(() => this.#render(), DEBOUNCE_MS);
	}

	async #render() {
		const input = this.#getInput();
		if (!input?.unique) return;

		this.#abort?.abort();
		const abort = new AbortController();
		this.#abort = abort;

		const { data, error } = await tryExecute(
			this,
			VisualEditorService.postVisualEditorRender({
				body: {
					unique: input.unique,
					culture: input.culture,
					segment: input.segment,
					values: input.values.map((v) => ({
						alias: v.alias,
						value: v.value,
						culture: v.culture,
						segment: v.segment,
					})),
				},
			}),
			{ abortSignal: abort.signal },
		);

		if (abort.signal.aborted) return; // A newer render superseded this one.

		if (error || !data) {
			console.error('[VisualEditor] Render failed', error);
			this.#onError?.();
			return;
		}

		this.#postRender(data.html);
	}

	override destroy() {
		if (this.#timer) clearTimeout(this.#timer);
		this.#abort?.abort();
		super.destroy();
	}
}
```

(If Task 4 reported a different generated method name or `tryExecute` does not accept an `abortSignal` option in this codebase, adjust: confirm with `grep -rn "abortSignal" src/packages/core/resources/`. If unsupported, drop the third arg and rely solely on the `abort.signal.aborted` guard after the await — the in-flight request still completes but its result is discarded.)

- [ ] **Step 2: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 3: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-render.controller.ts
git commit -m "feat(visual-editor): debounced latest-wins render controller (client)"
```

---

### Task 6: Wire render into the element + SignalR suppress-self-reload guard

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/visual-editor-signalr.controller.ts`
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

- [ ] **Step 1: Add a suppress-self-reload guard to the SignalR controller**

In `visual-editor-signalr.controller.ts`, add a field and a method, and guard the callback. Replace the class body's field/constructor region:

```typescript
	#connection?: HubConnection;
	#onRefreshed: (documentKey: string) => void;

	constructor(host: UmbControllerHost, onRefreshed: (documentKey: string) => void) {
		super(host);
		this.#onRefreshed = onRefreshed;
	}
```

with:

```typescript
	#connection?: HubConnection;
	#onRefreshed: (documentKey: string) => void;
	#suppressUntil = 0;

	constructor(host: UmbControllerHost, onRefreshed: (documentKey: string) => void) {
		super(host);
		this.#onRefreshed = (documentKey) => {
			if (Date.now() < this.#suppressUntil) return; // Our own save already painted the DOM.
			onRefreshed(documentKey);
		};
	}

	/** Ignore the next `refreshed` event(s) for the given window — used right after a local save. */
	suppressSelfReload(durationMs = 4000) {
		this.#suppressUntil = Date.now() + durationMs;
	}
```

(`this.#connection.on('refreshed', this.#onRefreshed)` already references the wrapped field — no change needed at the `connect()` call site.)

- [ ] **Step 2: Instantiate the render controller in the element**

In `document-workspace-view-visual-editor.element.ts`, add to the imports (after the SignalR controller import):

```typescript
import { UmbVisualEditorRenderController } from './visual-editor-render.controller.js';
```

Add the field next to `#structures` (near line 76):

```typescript
	#render = new UmbVisualEditorRenderController(this, {
		getInput: () => {
			const unique = this.#workspaceContext?.getUnique();
			if (!unique) return undefined;
			return {
				unique,
				culture: this.#currentVariantId?.culture ?? undefined,
				segment: this.#currentVariantId?.segment ?? undefined,
				values: this.#getAllValues().map((v) => ({ alias: v.alias, value: v.value })),
			};
		},
		postRender: (html) => this.#postToIframe({ type: 'umb:ve:render', html }),
		onError: () => console.warn('[VisualEditor] preview out of date — keeping last good DOM'),
	});
```

- [ ] **Step 3: Trigger a render after every value mutation**

Add a single private helper (place it next to `#setPropertyValue`, near line 472):

```typescript
	#afterMutation() {
		this.#signalR.suppressSelfReload();
		this.#render.requestRender();
	}
```

Then call `this.#afterMutation();` immediately after each `await this.#setPropertyValue(...)` in the following methods (verified mutation sites): `#handlePropertySubmit`, `#handleBlockSubmit`, `#addBlockToProperty` (covers `#onBlockAdd` and `#onBlockAddToProperty`), `#onBlockAddToArea`, `#handleClipboardPaste`, `#onBlockDelete`, `#onBlockMove`, `#onBlockReorder`.

Example — `#onBlockReorder` becomes:

```typescript
	async #onBlockReorder(blockKey: string, toIndex: number) {
		const found = this.#findBlock(blockKey);
		if (!found) return;

		const updatedValue = reorderBlockInValue(found.blockValue, blockKey, toIndex);
		await this.#setPropertyValue(found.propertyAlias, updatedValue);
		this.#afterMutation();
	}
```

Verify zero `#setPropertyValue` call sites are left without a following `#afterMutation()` (other than inside `#afterMutation` itself):

Run: `grep -n "#setPropertyValue\|#afterMutation" src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

(The optimistic `umb:ve:update-property-text` post in `#handlePropertySubmit` stays — it paints instantly; the debounced render then replaces the region with authoritative Razor output.)

- [ ] **Step 4: Build**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0.

- [ ] **Step 5: Commit**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/
git commit -m "feat(visual-editor): route edits through partial re-render, suppress self-reload after save"
```

---

### Task 7: Guest script — morphdom + re-runnable initRegions + umb:ve:render

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/package.json` (add morphdom dependency)
- Modify: `src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts`

- [ ] **Step 1: Add morphdom as a dependency**

Run:
```bash
cd src/Umbraco.Web.UI.Client
npm install morphdom@2.7.7
```

Expected: `morphdom` added to `dependencies` in `package.json`. (esbuild bundles it directly into the IIFE — no external-wrapper package is needed because `injected.ts` is a standalone esbuild bundle, not import-map based.)

- [ ] **Step 2: Import morphdom at the top of injected.ts**

In `src/apps/visual-editor/injected.ts`, add as the first line inside the IIFE (immediately after `'use strict';`), before the `PARENT_ORIGIN` derivation:

```typescript
	// morphdom is bundled by esbuild into this IIFE.
```

and at the very top of the file (module scope, above the IIFE):

```typescript
import morphdom from 'morphdom';
```

- [ ] **Step 3: Extract per-node setup into a re-runnable `initRegions()`**

The document-level `click`, `mouseover`, and `message` listeners are registered once at load and survive a DOM morph — leave them where they are. The per-node setup must be re-runnable. Wrap the per-node setup blocks (baseline outlines at lines ~195-204, action-bar attachment at lines ~417-419, the drag-to-sort IIFE at lines ~610-790, plus the existing `insertAddButtons()` and `discoverRegions()` calls) into one function. Define it once:

```typescript
	function initRegions() {
		applyBaselineOutlines();
		attachActionBars();
		setupDragSort();
		insertAddButtons();
		discoverRegions();
	}
```

Refactor the existing inline blocks into the named helpers it calls:
- `applyBaselineOutlines()` — the `document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach(...)` outline block (currently inline ~195-204).
- `attachActionBars()` — the `document.querySelectorAll<HTMLElement>(BLOCK_SELECTOR).forEach((block) => block.appendChild(createActionBar(block)));` block (currently inline ~417-419). Guard against duplicates: `if (block.querySelector('[' + ACTION_BAR_ATTR + ']')) return;` before appending.
- `setupDragSort()` — convert the existing drag-to-sort IIFE (`(function () { ... })();` at ~610-790) into a named `function setupDragSort() { ... }` (remove the self-invocation).
- `insertAddButtons()` and `discoverRegions()` already exist as functions — no change.

Replace the two final bootstrap calls at the end of the file (currently `insertAddButtons(); discoverRegions();`, lines ~1072-1073) with:

```typescript
	initRegions();
```

(`insertAddButtons()` already removes stale add-buttons before inserting, so it is safe to re-run. `createActionBar` attachment is now duplicate-guarded above.)

- [ ] **Step 4: Handle the `umb:ve:render` message**

In the `window.addEventListener('message', ...)` handler (after the existing `umb:ve:select-region` branch, before the handler closes), add:

```typescript
		if (evt.data.type === 'umb:ve:render' && typeof evt.data.html === 'string') {
			const previouslySelected = selectedId;

			const doc = new DOMParser().parseFromString(evt.data.html, 'text/html');
			if (doc.body) {
				morphdom(document.body, doc.body, { childrenOnly: true });
			}

			initRegions();

			if (previouslySelected) {
				document.querySelectorAll<HTMLElement>(ALL_SELECTOR).forEach((el) => {
					if (getRegionId(el) === previouslySelected) {
						selectedId = previouslySelected;
						applyOutline(el, `2px solid ${COLOR_SELECTED}`, '-2px');
					}
				});
			}
		}
```

Also update the file-header doc comment: under "## PostMessage protocol (received from parent)" add:

```
 * - `umb:ve:render` — Full re-rendered page HTML; morph <body> in place and re-init regions
```

- [ ] **Step 5: Build (rebuilds the guest IIFE via the vite plugin)**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: exits 0; `wwwroot/umbraco/backoffice/apps/visual-editor/injected.js` is regenerated (the esbuild plugin runs on `buildStart`).

- [ ] **Step 6: Commit**

```bash
git add src/Umbraco.Web.UI.Client/package.json src/Umbraco.Web.UI.Client/package-lock.json src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts
git commit -m "feat(visual-editor): morph preview DOM on partial re-render (guest)"
```

---

### Task 8: Docs + final verification

**Files:**
- Modify: `docs/plans/2026-06-11-visual-editor-partial-rerender-design.md`
- Modify: `docs/plans/visual-page-builder.md`

- [ ] **Step 1: Full client build + lint**

```bash
cd src/Umbraco.Web.UI.Client
npm run build
npm run lint
```

Expected: build exits 0; lint reports no NEW errors in the visual-editor files (the guest message keys `umb:ve:render` etc. are already exempted from naming-convention lint per commit `80c0e8e8`; if a new key trips the rule, extend that exemption the same way).

- [ ] **Step 2: Full solution build + backend tests**

```bash
dotnet build umbraco.sln
dotnet test tests/Umbraco.Tests.Integration --filter "FullyQualifiedName~VisualEditorContentFactoryTests|FullyQualifiedName~VisualEditorRenderServiceTests"
```

Expected: 0 build errors (pre-existing StyleCop warnings are out of scope); both tests pass.

- [ ] **Step 3: Manual smoke pass** (run the site: `dotnet run --project src/Umbraco.Web.UI`, backoffice at `https://localhost:44339/umbraco`)

Checklist:
1. Open a document with a template in the visual editor tab — iframe loads, regions outlined.
2. Edit a flagged text property in the sidebar modal → the region updates: instant optimistic paint, then the debounced authoritative render (~500ms) replaces it with Razor output. No full reload; scroll position preserved.
3. Edit a block's content/settings → the block re-renders in place after the debounce.
4. Add a block, delete a block, drag-reorder, move into a grid area → each reflects via morph, no reload.
5. Save → no flicker/self-reload (suppress guard); a change from ANOTHER tab/user still triggers a full refresh.
6. Selection highlight is restored on the same region after each morph.
7. Verify in BOTH built mode (same-origin) and `npm run dev` Vite mode (cross-origin) that the render round-trips and morph applies.
8. Trigger a render failure (e.g. stop the server briefly) → DOM is kept, a console warning logs, no blank page.

- [ ] **Step 4: Mark the design implemented and update the feature plan**

In `docs/plans/2026-06-11-visual-editor-partial-rerender-design.md` replace:

```markdown
**Status**: Approved design; **gated on conversion spike** (must pass before the full implementation plan is written)
```

with:

```markdown
**Status**: Implemented (spike passed 2026-06-11; see `2026-06-11-visual-editor-partial-rerender-plan.md`). Built via cache-node override + `IPublishedContentFactory` rather than a decorator — see the plan's "Deliberate deviation" note.
```

In `docs/plans/visual-page-builder.md`, under Phase 3, replace:

```markdown
**Still to build:**
- Partial re-render API endpoint (extends BlockPreview pattern)
- `PropertyOverridePublishedContent` decorator for rendering with unsaved values
- Guest script DOM patching for partial updates
```

with:

```markdown
**Done (2026-06-11):**
- Partial re-render API endpoint (`POST /umbraco/management/api/v1/visual-editor/render`)
- Unsaved-value rendering via `IVisualEditorContentFactory` (cache-node override) + `IVisualEditorRenderService`
- Guest script DOM patching via morphdom + re-runnable `initRegions()`; self-reload suppressed after local save

Phase 3 is complete.
```

- [ ] **Step 5: Commit**

```bash
git add docs/plans/2026-06-11-visual-editor-partial-rerender-design.md docs/plans/visual-page-builder.md
git commit -m "docs(visual-editor): mark partial re-render implemented (Phase 3 complete)"
```

---

## Self-review notes

- **Spec coverage:** design "Server components" → Tasks 1–3; "Client components" → Tasks 5–7; SignalR suppress guard → Task 6 Step 1; error handling (keep DOM + notice) → Task 5 `onError` + Task 7 (morph only on success); debounce + latest-wins → Task 5; morphdom body morph → Task 7; backend integration test → Tasks 1–2.
- **Deviation logged:** decorator → cache-node override (justified at top + Task 8 doc updates).
- **Known gaps (acceptable):** full variant matrix beyond the requested culture/segment is not exhaustively handled (override applies to the one requested variant — matches design "variant-aware: override applies to the requested culture/segment"); empty-block-list container annotation remains preview-cookie-gated in the cshtml, so empty-list "add content" placeholders may not appear in the API render scope — populated regions and property spans are unaffected. Note for a follow-up if needed.
- **Verify-at-execution points (flagged inline, not placeholders):** exact generated client method name (Task 4 Step 2); `tryExecute` abortSignal support (Task 5 Step 1); `ManagementApiControllerBase` name (Task 3 Step 2); DI registration extension locations (Task 1 Step 5, Task 2 Step 7); test-builder helper API (Task 2 Step 4).
