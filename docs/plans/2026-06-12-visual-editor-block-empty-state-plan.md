# Visual Editor — Framework-Emitted Empty-Block Affordance — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the visual-editor empty-block "Add content" affordance fully automatic via the block-rendering helpers, removing the per-view template boilerplate and the ViewData plumbing.

**Architecture:** The block helpers (`BlockListTemplateExtensions`, `BlockGridTemplateExtensions`, `SingleBlockTemplateExtensions` in `Umbraco.Web.Common`) emit an annotated empty container `<div class="umb-block-{list,grid,single}" data-umb-block-property="{alias}">` themselves — but only from the **alias-bearing overloads** (which carry the alias + `PropertyType.EditableInVisualEditor` even when the value is empty), and only when `VisualEditorPropertyTracker.IsEnabled` and the property is editable-in-VE. A shared `BlockEmptyState` helper DRYs the gating + markup. The default views revert to their plain form, and the ViewData plumbing is deleted. No changes to Core models / value creators / converters.

**Tech Stack:** C# / ASP.NET Core Razor helpers (`Umbraco.Web.Common`), Razor views (`Umbraco.Web.UI`, embedded `Umbraco.Core`), TypeScript guest (`injected.ts`) + Lit element (backoffice client). Working dir for ALL tasks: `D:/CMS/Umbraco-CMS/.worktrees/feature-visual-editor`.

**Spec:** `docs/plans/2026-06-12-visual-editor-block-empty-state-design.md`

**Standing instruction:** the user asked for **no commits yet**. Implement and verify each task; leave changes in the working tree **uncommitted**. The "Commit" steps below are written for completeness but are GATED — do not run them until the user approves committing. Report each task's diff for review instead.

**Verified facts:**
- Empty block values resolve to the shared singletons `BlockListModel.Empty` / `BlockGridModel.Empty`; an empty single block converts to `null`. Hence the alias can only come from the alias-bearing overloads, not the model. (No model/creator/converter changes.)
- `IPublishedPropertyType` exposes `string Alias` and `bool EditableInVisualEditor` (default `false`) — `src/Umbraco.Core/Models/PublishedContent/IPublishedPropertyType.cs:27,52`.
- `VisualEditorPropertyTracker.IsEnabled` is a public static in `Umbraco.Cms.Core.Models.PublishedContent`.
- `SingleBlockValue : BlockValue<SingleBlockLayoutItem>` with `PropertyEditorAlias => Constants.PropertyEditors.Aliases.SingleBlock` — so single-block add reuses `addBlockToValue` with the single-block schema alias.
- The guest already has empty-container branches for `.umb-block-list` and `.umb-block-grid` reading `dataset.umbBlockProperty`; there is **no** single-block handling.

---

### Task 1: Shared empty-state helper

**Files:**
- Create: `src/Umbraco.Web.Common/Extensions/BlockEmptyState.cs`
- Test: `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockEmptyStateTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockEmptyStateTests.cs`:

```csharp
using Microsoft.AspNetCore.Html;
using NUnit.Framework;
using System.IO;
using System.Text.Encodings.Web;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class BlockEmptyStateTests
{
    [TearDown]
    public void TearDown() => VisualEditorPropertyTracker.Disable();

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    [Test]
    public void Returns_Annotated_Container_When_Enabled_And_Editable()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", "bodyText", editableInVisualEditor: true);
        var html = Render(result);
        Assert.That(html, Does.Contain("class=\"umb-block-list\""));
        Assert.That(html, Does.Contain("data-umb-block-property=\"bodyText\""));
    }

    [Test]
    public void Returns_Empty_When_Tracker_Disabled()
    {
        VisualEditorPropertyTracker.Disable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", "bodyText", editableInVisualEditor: true);
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public void Returns_Empty_When_Not_Editable()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-grid", "bodyText", editableInVisualEditor: false);
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public void Returns_Empty_When_Alias_Missing()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", string.Empty, editableInVisualEditor: true);
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public void Encodes_Alias_And_Class()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = BlockEmptyState.Container("umb-block-list", "a\"b", editableInVisualEditor: true);
        var html = Render(result);
        Assert.That(html, Does.Not.Contain("a\"b"));
        Assert.That(html, Does.Contain("a&quot;b").Or.Contain("a&#x22;b"));
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~BlockEmptyStateTests"`
Expected: FAIL (build error — `BlockEmptyState` does not exist).

- [ ] **Step 3: Implement `BlockEmptyState`**

Create `src/Umbraco.Web.Common/Extensions/BlockEmptyState.cs`:

```csharp
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

/// <summary>
/// Produces the annotated empty container the visual editor uses to offer an "add content"
/// affordance on an empty, editable block property. Returns empty content outside the visual editor.
/// </summary>
internal static class BlockEmptyState
{
    /// <summary>
    /// Returns an annotated empty container (<c>&lt;div class="{cssClass}" data-umb-block-property="{alias}"&gt;</c>)
    /// when the property is editable in the visual editor and the visual editor is active; otherwise empty content.
    /// </summary>
    public static IHtmlContent Container(string cssClass, string propertyAlias, bool editableInVisualEditor)
    {
        if (!editableInVisualEditor
            || string.IsNullOrEmpty(propertyAlias)
            || !VisualEditorPropertyTracker.IsEnabled)
        {
            return HtmlString.Empty;
        }

        var encodedClass = HtmlEncoder.Default.Encode(cssClass);
        var encodedAlias = HtmlEncoder.Default.Encode(propertyAlias);
        return new HtmlString($"<div class=\"{encodedClass}\" data-umb-block-property=\"{encodedAlias}\"></div>");
    }
}
```

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~BlockEmptyStateTests"`
Expected: PASS (5 passed).

- [ ] **Step 5: Commit (GATED — only if the user has approved committing)**

```bash
git add src/Umbraco.Web.Common/Extensions/BlockEmptyState.cs tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockEmptyStateTests.cs
git commit -m "feat(visual-editor): shared empty-state container helper for block properties"
```

---

### Task 2: Block list helper emits the affordance; remove ViewData plumbing

**Files:**
- Modify: `src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs`
- Test: `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockListTemplateExtensionsTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockListTemplateExtensionsTests.cs`:

```csharp
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class BlockListTemplateExtensionsTests
{
    [TearDown]
    public void TearDown() => VisualEditorPropertyTracker.Disable();

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static IPublishedProperty EmptyEditableProperty(string alias, bool editable)
    {
        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(x => x.Alias).Returns(alias);
        propertyType.SetupGet(x => x.EditableInVisualEditor).Returns(editable);

        var property = new Mock<IPublishedProperty>();
        property.SetupGet(x => x.Alias).Returns(alias);
        property.SetupGet(x => x.PropertyType).Returns(propertyType.Object);
        property.Setup(x => x.GetValue(null, null)).Returns(BlockListModel.Empty);
        return property.Object;
    }

    [Test]
    public async Task Empty_Editable_Property_In_VisualEditor_Emits_Annotated_Container()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockListHtmlAsync(EmptyEditableProperty("bodyText", editable: true));
        var html = Render(result);
        Assert.That(html, Does.Contain("class=\"umb-block-list\""));
        Assert.That(html, Does.Contain("data-umb-block-property=\"bodyText\""));
    }

    [Test]
    public async Task Empty_NonEditable_Property_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockListHtmlAsync(EmptyEditableProperty("bodyText", editable: false));
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public async Task Empty_Property_Outside_VisualEditor_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Disable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockListHtmlAsync(EmptyEditableProperty("bodyText", editable: true));
        Assert.That(Render(result), Is.Empty);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~BlockListTemplateExtensionsTests"`
Expected: FAIL — the current helper short-circuits empty to `HtmlString.Empty` (no container), so the first test fails.

- [ ] **Step 3: Rewrite `BlockListTemplateExtensions.cs`**

Replace the full contents of `src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs` with:

```csharp
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions;

public static class BlockListTemplateExtensions
{
    public const string DefaultFolder = "blocklist/";
    public const string DefaultTemplate = "default";

    #region Async

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return HtmlString.Empty;
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockListHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockListHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockListHtmlAsync(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);
    }

    private static async Task<IHtmlContent> GetBlockListHtmlAsync(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null || model.Count == 0)
        {
            return BlockEmptyState.Container("umb-block-list", propertyAlias, editableInVisualEditor);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }

    #endregion

    #region Sync

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, BlockListModel? model, string template = DefaultTemplate)
    {
        if (model?.Count == 0)
        {
            return HtmlString.Empty;
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockListHtml(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockListHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockListHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockListHtml(html, property.GetValue() as BlockListModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);
    }

    private static IHtmlContent GetBlockListHtml(IHtmlHelper html, BlockListModel? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null || model.Count == 0)
        {
            return BlockEmptyState.Container("umb-block-list", propertyAlias, editableInVisualEditor);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }

    #endregion

    private static string DefaultFolderTemplate(string template) => $"{DefaultFolder}{template}";

    private static IPublishedProperty GetRequiredProperty(IPublishedContent contentItem, string propertyAlias)
    {
        ArgumentNullException.ThrowIfNull(propertyAlias);

        if (string.IsNullOrWhiteSpace(propertyAlias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(propertyAlias));
        }

        IPublishedProperty? property = contentItem.GetProperty(propertyAlias);
        if (property == null)
        {
            throw new InvalidOperationException("No property type found with alias " + propertyAlias);
        }

        return property;
    }
}
```

This removes `PropertyAliasViewDataKey`, `WithPropertyAlias`, the `Microsoft.AspNetCore.Mvc.ViewFeatures` using, and the old alias-via-ViewData private overloads.

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~BlockListTemplateExtensionsTests"`
Expected: PASS (3 passed).

- [ ] **Step 5: Build Web.Common**

Run: `dotnet build src/Umbraco.Web.Common/Umbraco.Web.Common.csproj`
Expected: 0 errors.

- [ ] **Step 6: Commit (GATED)**

```bash
git add src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockListTemplateExtensionsTests.cs
git commit -m "feat(visual-editor): block list helper emits empty-state affordance, drop ViewData plumbing"
```

---

### Task 3: Block grid helper emits the affordance; remove ViewData plumbing

**Files:**
- Modify: `src/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensions.cs`
- Test: `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensionsTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensionsTests.cs`:

```csharp
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class BlockGridTemplateExtensionsTests
{
    [TearDown]
    public void TearDown() => VisualEditorPropertyTracker.Disable();

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static IPublishedProperty EmptyEditableProperty(string alias, bool editable)
    {
        var emptyGrid = new BlockGridModel(new List<BlockGridItem>(), null);

        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(x => x.Alias).Returns(alias);
        propertyType.SetupGet(x => x.EditableInVisualEditor).Returns(editable);

        var property = new Mock<IPublishedProperty>();
        property.SetupGet(x => x.Alias).Returns(alias);
        property.SetupGet(x => x.PropertyType).Returns(propertyType.Object);
        property.Setup(x => x.GetValue(null, null)).Returns(emptyGrid);
        return property.Object;
    }

    [Test]
    public async Task Empty_Editable_Property_In_VisualEditor_Emits_Annotated_Container()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockGridHtmlAsync(EmptyEditableProperty("bodyText", editable: true));
        var html = Render(result);
        Assert.That(html, Does.Contain("class=\"umb-block-grid\""));
        Assert.That(html, Does.Contain("data-umb-block-property=\"bodyText\""));
    }

    [Test]
    public async Task Empty_NonEditable_Property_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockGridHtmlAsync(EmptyEditableProperty("bodyText", editable: false));
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public async Task Empty_Property_Outside_VisualEditor_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Disable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockGridHtmlAsync(EmptyEditableProperty("bodyText", editable: true));
        Assert.That(Render(result), Is.Empty);
    }
}
```

(Note: `new BlockGridModel(new List<BlockGridItem>(), null)` is used instead of `BlockGridModel.Empty` because `Empty` has `Count == 0` and either works; the explicit list keeps the test independent of the singleton.)

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~BlockGridTemplateExtensionsTests"`
Expected: FAIL (no container emitted by current helper).

- [ ] **Step 3: Edit `BlockGridTemplateExtensions.cs`**

In `src/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensions.cs`:

(a) Remove the `using Microsoft.AspNetCore.Mvc.ViewFeatures;` line.

(b) Remove the `PropertyAliasViewDataKey` const + its XML doc (lines 20-24).

(c) Replace the async property/content overloads + private method (lines 51-66) — change the property overloads to pass the alias **and** editable flag, and rewrite the private method to emit the empty-state:

```csharp
    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockGridHtmlAsync(html, property.GetValue() as BlockGridModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockGridHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockGridHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty prop = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockGridHtmlAsync(html, prop.GetValue() as BlockGridModel, template, prop.Alias, prop.PropertyType.EditableInVisualEditor);
    }

    private static async Task<IHtmlContent> GetBlockGridHtmlAsync(IHtmlHelper html, BlockGridModel? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null || model.Count == 0)
        {
            return BlockEmptyState.Container("umb-block-grid", propertyAlias, editableInVisualEditor);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }
```

(d) Mirror the same change in the sync region (lines 104-118):

```csharp
    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockGridHtml(html, property.GetValue() as BlockGridModel, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    /// <inheritdoc cref="GetBlockGridHtmlAsync(Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper,Umbraco.Cms.Core.Models.Blocks.BlockGridModel?,string)"/>
    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockGridHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockGridHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty prop = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockGridHtml(html, prop.GetValue() as BlockGridModel, template, prop.Alias, prop.PropertyType.EditableInVisualEditor);
    }

    private static IHtmlContent GetBlockGridHtml(IHtmlHelper html, BlockGridModel? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null || model.Count == 0)
        {
            return BlockEmptyState.Container("umb-block-grid", propertyAlias, editableInVisualEditor);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }
```

(e) Remove the now-unused `WithPropertyAlias` private method (lines 139-140). Leave `GetBlockGridItemsHtmlAsync`/`GetBlockGridItemAreasHtmlAsync`/etc. and `GetRequiredProperty` unchanged. The model-only `GetBlockGridHtmlAsync(BlockGridModel? model, ...)` overload (lines 41-49) keeps its `model?.Count == 0 → HtmlString.Empty` form unchanged.

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~BlockGridTemplateExtensionsTests"`
Expected: PASS (3 passed).

- [ ] **Step 5: Build Web.Common**

Run: `dotnet build src/Umbraco.Web.Common/Umbraco.Web.Common.csproj`
Expected: 0 errors.

- [ ] **Step 6: Commit (GATED)**

```bash
git add src/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensions.cs tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensionsTests.cs
git commit -m "feat(visual-editor): block grid helper emits empty-state affordance, drop ViewData plumbing"
```

---

### Task 4: Single block helper emits the affordance

**Files:**
- Modify: `src/Umbraco.Web.Common/Extensions/SingleBlockTemplateExtensions.cs`
- Test: `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/SingleBlockTemplateExtensionsTests.cs`

The single-block value is a `BlockListItem?`; empty = `null`. The alias-bearing overloads have the property even when the value is null.

- [ ] **Step 1: Write the failing test**

Create `tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/SingleBlockTemplateExtensionsTests.cs`:

```csharp
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class SingleBlockTemplateExtensionsTests
{
    [TearDown]
    public void TearDown() => VisualEditorPropertyTracker.Disable();

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static IPublishedProperty NullEditableProperty(string alias, bool editable)
    {
        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(x => x.Alias).Returns(alias);
        propertyType.SetupGet(x => x.EditableInVisualEditor).Returns(editable);

        var property = new Mock<IPublishedProperty>();
        property.SetupGet(x => x.Alias).Returns(alias);
        property.SetupGet(x => x.PropertyType).Returns(propertyType.Object);
        property.Setup(x => x.GetValue(null, null)).Returns((object?)null);
        return property.Object;
    }

    [Test]
    public async Task Empty_Editable_Single_Block_In_VisualEditor_Emits_Annotated_Container()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockHtmlAsync(NullEditableProperty("hero", editable: true));
        var html = Render(result);
        Assert.That(html, Does.Contain("class=\"umb-single-block\""));
        Assert.That(html, Does.Contain("data-umb-block-property=\"hero\""));
    }

    [Test]
    public async Task Empty_NonEditable_Single_Block_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Enable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockHtmlAsync(NullEditableProperty("hero", editable: false));
        Assert.That(Render(result), Is.Empty);
    }

    [Test]
    public async Task Empty_Single_Block_Outside_VisualEditor_Emits_Nothing()
    {
        VisualEditorPropertyTracker.Disable();
        IHtmlContent result = await Mock.Of<IHtmlHelper>().GetBlockHtmlAsync(NullEditableProperty("hero", editable: true));
        Assert.That(Render(result), Is.Empty);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~SingleBlockTemplateExtensionsTests"`
Expected: FAIL (current helper returns `HtmlString.Empty` for null model).

- [ ] **Step 3: Edit `SingleBlockTemplateExtensions.cs`**

Change the alias-bearing overloads to pass the alias + editable flag through to a private method that emits the empty-state. The model-only overloads keep their `model is null → HtmlString.Empty` behaviour.

Replace the async property/content overloads (lines 27-37) with:

```csharp
    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => await GetBlockHtmlAsync(html, property.GetValue() as BlockListItem, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => await GetBlockHtmlAsync(html, contentItem, propertyAlias, DefaultTemplate);

    public static async Task<IHtmlContent> GetBlockHtmlAsync(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return await GetBlockHtmlAsync(html, property.GetValue() as BlockListItem, template, property.Alias, property.PropertyType.EditableInVisualEditor);
    }

    private static async Task<IHtmlContent> GetBlockHtmlAsync(IHtmlHelper html, BlockListItem? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null)
        {
            return BlockEmptyState.Container("umb-single-block", propertyAlias, editableInVisualEditor);
        }

        return await html.PartialAsync(DefaultFolderTemplate(template), model);
    }
```

Replace the sync property/content overloads (lines 52-62) with:

```csharp
    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, IPublishedProperty property, string template = DefaultTemplate)
        => GetBlockHtml(html, property.GetValue() as BlockListItem, template, property.Alias, property.PropertyType.EditableInVisualEditor);

    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias)
        => GetBlockHtml(html, contentItem, propertyAlias, DefaultTemplate);

    public static IHtmlContent GetBlockHtml(this IHtmlHelper html, IPublishedContent contentItem, string propertyAlias, string template)
    {
        IPublishedProperty property = GetRequiredProperty(contentItem, propertyAlias);
        return GetBlockHtml(html, property.GetValue() as BlockListItem, template, property.Alias, property.PropertyType.EditableInVisualEditor);
    }

    private static IHtmlContent GetBlockHtml(IHtmlHelper html, BlockListItem? model, string template, string propertyAlias, bool editableInVisualEditor)
    {
        if (model is null)
        {
            return BlockEmptyState.Container("umb-single-block", propertyAlias, editableInVisualEditor);
        }

        return html.Partial(DefaultFolderTemplate(template), model);
    }
```

Leave the model-only `GetBlockHtmlAsync(BlockListItem? model, ...)` / `GetBlockHtml(BlockListItem? model, ...)` overloads (lines 17-25, 42-50), `SingleBlockPartialWithFallback`, `DefaultFolderTemplate`, and `GetRequiredProperty` unchanged.

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~SingleBlockTemplateExtensionsTests"`
Expected: PASS (3 passed).

- [ ] **Step 5: Build + commit (GATED)**

```bash
dotnet build src/Umbraco.Web.Common/Umbraco.Web.Common.csproj
git add src/Umbraco.Web.Common/Extensions/SingleBlockTemplateExtensions.cs tests/Umbraco.Tests.UnitTests/Umbraco.Web.Common/Extensions/SingleBlockTemplateExtensionsTests.cs
git commit -m "feat(visual-editor): single block helper emits empty-state affordance"
```

---

### Task 5: Revert the views to their plain form

**Files:**
- Modify: `src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml`
- Modify: `src/Umbraco.Web.UI/Views/Partials/blockgrid/default.cshtml`
- Modify: `src/Umbraco.Core/EmbeddedResources/BlockGrid/default.cshtml`

These no longer carry empty-state logic — the helper handles it. (The `singleblock/default.cshtml` was never modified and stays as-is: the helper now handles the empty/null case before the partial is invoked, so the partial only ever renders a non-null block.)

- [ ] **Step 1: Revert `blocklist/default.cshtml`**

Replace the full contents of `src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml` with:

```razor
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListModel>
@{
    if (Model?.Any() != true) { return; }
}
<div class="umb-block-list">
    @foreach (var block in Model)
    {
        if (block?.ContentKey == null) { continue; }
        var data = block.Content;

        <div data-umb-block-key="@block.ContentKey" data-umb-content-type="@data.ContentType.Alias">
            @await Html.PartialAsync("blocklist/Components/" + data.ContentType.Alias, block)
        </div>
    }
</div>
```

(Note: the per-block `data-umb-block-key`/`data-umb-content-type` annotations on populated blocks are retained — they were present before the empty-state work and are needed for selecting existing blocks. Only the empty-state `data-umb-block-property` + ViewData read are removed.)

- [ ] **Step 2: Revert `blockgrid/default.cshtml`**

Replace the full contents of `src/Umbraco.Web.UI/Views/Partials/blockgrid/default.cshtml` with:

```razor
@using Umbraco.Extensions
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridModel>
@{
    if (Model?.Any() != true) { return; }
    var gridColumns = Model.GridColumns?.ToString() ?? "12";
}

<div class="umb-block-grid" data-grid-columns="@(gridColumns)" style="--umb-block-grid--grid-columns: @(gridColumns);">
    @await Html.GetBlockGridItemsHtmlAsync(Model)
</div>
```

- [ ] **Step 3: Revert embedded `BlockGrid/default.cshtml`**

Replace the full contents of `src/Umbraco.Core/EmbeddedResources/BlockGrid/default.cshtml` with the identical plain form:

```razor
@using Umbraco.Extensions
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridModel>
@{
    if (Model?.Any() != true) { return; }
    var gridColumns = Model.GridColumns?.ToString() ?? "12";
}

<div class="umb-block-grid" data-grid-columns="@(gridColumns)" style="--umb-block-grid--grid-columns: @(gridColumns);">
    @await Html.GetBlockGridItemsHtmlAsync(Model)
</div>
```

- [ ] **Step 4: Confirm no remaining references to the removed ViewData keys**

Run: `grep -rn "PropertyAliasViewDataKey\|umbBlockListPropertyAlias\|umbBlockGridPropertyAlias" src/`
Expected: zero hits (the consts were removed in Tasks 2-3 and the views no longer read them).

- [ ] **Step 5: Build Web.UI (validates compile; Razor is runtime-compiled)**

Run: `dotnet build src/Umbraco.Web.UI/Umbraco.Web.UI.csproj`
Expected: 0 errors. (Stop any running dev instance first to avoid DLL file-locks.)

- [ ] **Step 6: Commit (GATED)**

```bash
git add src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml src/Umbraco.Web.UI/Views/Partials/blockgrid/default.cshtml src/Umbraco.Core/EmbeddedResources/BlockGrid/default.cshtml
git commit -m "refactor(visual-editor): revert block view empty-state boilerplate (now framework-emitted)"
```

---

### Task 6: Guest — single-block empty-container branch

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts`

The guest already handles empty `.umb-block-list` and `.umb-block-grid` containers (unchanged — the helper now emits the same `data-umb-block-property` markup). Add a parallel branch for the single-block container class `umb-single-block`.

- [ ] **Step 1: Add the single-block empty-container branch**

In `insertAddButtons()`, immediately after the existing empty `.umb-block-grid` branch (the block that does `document.querySelectorAll<HTMLElement>('.umb-block-grid').forEach(...)`), add:

```typescript
		// Empty single block at root level. The container carries data-umb-block-property
		// (emitted by the single block helper in visual-editor mode).
		document.querySelectorAll<HTMLElement>('.umb-single-block').forEach((single) => {
			if (single.querySelector(BLOCK_SELECTOR)) return; // Has a block
			if (single.querySelector(`[${ADD_BTN_ATTR}]`)) return; // Already handled

			const propertyAlias = single.dataset.umbBlockProperty || '';
			if (!propertyAlias) return;

			single.appendChild(
				createEmptyPlaceholder(() => {
					send({ type: 'umb:ve:block-add-to-property', propertyAlias, insertIndex: 0 });
				}),
			);
		});
```

Also update the file-header doc comment line for `data-umb-block-property` to read: `Property alias on a block list, block grid, or single block container (empty-state block creation)`.

- [ ] **Step 2: Build the client**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: tsc exits 0. (Allow up to 600000ms.)

- [ ] **Step 3: Commit (GATED)**

```bash
git add src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts
git commit -m "feat(visual-editor): single block empty-state add-content affordance (guest)"
```

---

### Task 7: Element — single-block-aware add

**Files:**
- Modify: `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts`

When the guest sends `umb:ve:block-add-to-property` for a single-block property, the element must create a single-block-shaped value (layout key `Umbraco.SingleBlock`). Today `#resolveBlockSchemaAlias` only maps grid vs list; extend it for single block so `addBlockToValue` writes the right layout key.

- [ ] **Step 1: Confirm the single-block client constants**

Run: `grep -rn "PROPERTY_EDITOR_SCHEMA_ALIAS\|PROPERTY_EDITOR_UI_ALIAS" src/Umbraco.Web.UI.Client/src/packages/block/block-single/`
Expected: find the exported constants for the single block editor — the schema alias (value `Umbraco.SingleBlock`) and the UI alias (value `Umb.PropertyEditorUi.BlockSingle` or similar). Note their exact exported names and the import path (`@umbraco-cms/backoffice/block-single`). If the names differ from those used below, substitute the real names.

- [ ] **Step 2: Extend `#resolveBlockSchemaAlias`**

Add the import (next to the existing block-grid import):

```typescript
import {
	UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS,
} from '@umbraco-cms/backoffice/block-single';
```

Replace `#resolveBlockSchemaAlias` with:

```typescript
	#resolveBlockSchemaAlias(propertyAlias: string): string {
		const editorUiAlias = this.#structures.getDocumentProperty(propertyAlias)?.editorUiAlias ?? '';
		if (editorUiAlias === UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS) {
			return UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS;
		}
		if (editorUiAlias === UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS) {
			return UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS;
		}
		return UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS;
	}
```

(`addBlockToValue` keys its grid-specific layout logic on `UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS`; for the single-block alias it falls through to the plain list-shaped layout item, which matches `SingleBlockValue`'s `BlockValue<SingleBlockLayoutItem>` structure — one block under the `Umbraco.SingleBlock` layout key, no columnSpan/rowSpan.)

- [ ] **Step 3: Build the client**

Run: `cd src/Umbraco.Web.UI.Client && npm run build`
Expected: tsc exits 0. (Allow up to 600000ms.) If the single-block constant names differ, fix the import to the real names found in Step 1.

- [ ] **Step 4: Commit (GATED)**

```bash
git add src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts
git commit -m "feat(visual-editor): single-block-aware add for empty single block properties"
```

---

### Task 8: Final verification + mark spec implemented

- [ ] **Step 1: Unit tests**

Run: `dotnet test tests/Umbraco.Tests.UnitTests --filter "FullyQualifiedName~TemplateExtensionsTests|FullyQualifiedName~BlockEmptyStateTests"`
Expected: all helper + empty-state tests pass.

- [ ] **Step 2: Full client build + lint**

```bash
cd src/Umbraco.Web.UI.Client
npm run build
npm run lint
```
Expected: build exits 0; lint reports no NEW errors in the visual-editor files (the `umb:ve:*` keys are already lint-exempt).

- [ ] **Step 3: Full solution build**

Run: `dotnet build umbraco.sln`
Expected: 0 errors (pre-existing StyleCop warnings out of scope).

- [ ] **Step 4: Manual smoke** (run the site, backoffice at https://localhost:44339/umbraco)

1. Empty editable block **list** property → preview shows the annotated empty container with an "Add content" button; clicking it adds a block.
2. Empty editable block **grid** property (e.g. Blogpost `bodyText`) → same.
3. Empty editable **single block** property → same; clicking adds exactly one block.
4. A **non-editable** empty block property → renders nothing, no affordance.
5. A custom template that renders a block property via `@Html.GetBlock*HtmlAsync(Model, "alias")` → affordance appears with **no template code** for the empty state.
6. Non-empty block properties render unchanged.

- [ ] **Step 5: Update the design doc status**

In `docs/plans/2026-06-12-visual-editor-block-empty-state-design.md` replace:

```markdown
**Status**: Approved design, pending implementation plan
```

with:

```markdown
**Status**: Implemented
```

- [ ] **Step 6: Commit (GATED)**

```bash
git add docs/plans/2026-06-12-visual-editor-block-empty-state-design.md
git commit -m "docs(visual-editor): mark framework-emitted empty-block affordance implemented"
```

---

## Self-review notes

- **Spec coverage:** Component 1 (helper emits container) → Tasks 2-4 + the `BlockEmptyState` helper (Task 1); Component 2 (alias-bearing overloads only, no model metadata) → Tasks 2-4 pass alias + `EditableInVisualEditor` from the property; Component 3 (single block) → Tasks 4, 6, 7; Component 4 (guest/element) → Tasks 6-7; Component 5 (revert views) → Task 5; Testing → unit tests in Tasks 1-4 + manual in Task 8.
- **Verify-at-execution (not placeholders):** the single-block client constant names (Task 7 Step 1) — exact exported names confirmed by grep before use.
- **No model/creator/converter changes** — consistent with the singleton finding.
- **Commits are GATED** per the user's "no commits yet" instruction — execute and review; commit only on approval.
