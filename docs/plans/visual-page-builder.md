# Visual Page Builder for Umbraco CMS

**Status**: PoC Complete — click-to-edit properties with live preview updates working end-to-end
**Date**: 2026-03-16
**Author**: Rick Butterfield + Claude

---

## Table of Contents

1. [Vision](#1-vision)
2. [Current State Analysis](#2-current-state-analysis)
3. [Architecture Overview](#3-architecture-overview)
4. [Component Design](#4-component-design)
   - 4.1 [Annotated Rendering Pipeline](#41-annotated-rendering-pipeline)
   - 4.2 [Visual Editor Frontend](#42-visual-editor-frontend)
   - 4.3 [Iframe Communication Protocol](#43-iframe-communication-protocol)
   - 4.4 [Partial Re-render API](#44-partial-re-render-api)
   - 4.5 [Inline Editing](#45-inline-editing)
   - 4.6 [Block Manipulation](#46-block-manipulation)
   - 4.7 [Workspace Integration](#47-workspace-integration)
5. [Data Flow](#5-data-flow)
6. [Phased Delivery](#6-phased-delivery)
7. [Technical Challenges & Mitigations](#7-technical-challenges--mitigations)
8. [Files to Create or Modify](#8-files-to-create-or-modify)
9. [Open Questions](#9-open-questions)
10. [Architecture Evolution: Standalone Window](#10-architecture-evolution-standalone-window)
    - 10.1 [Motivation](#101-motivation)
    - 10.2 [Architecture](#102-architecture-standalone-visual-editor-app)
    - 10.3 [File Structure](#103-file-structure)
    - 10.4 [Key Components](#104-key-components)
    - 10.5 [Opening the Visual Editor](#105-opening-the-visual-editor)
    - 10.6 [Cross-Tab Communication](#106-cross-tab-communication)
    - 10.7 [Property Editor Compatibility Risk](#107-property-editor-compatibility-risk)
    - 10.8 [Migration Path](#108-migration-path)
    - 10.9 [Comparison: Embedded vs Standalone](#109-comparison-embedded-vs-standalone)
    - 10.10 [Server-Side Changes](#1010-server-side-changes)
11. [Discussion Notes & Feedback](#11-discussion-notes--feedback)

---

## 1. Vision

Replace the split editing/preview experience with a unified visual page builder where:

- The **full page** is rendered (master template, layout, partials) inside the backoffice
- Each **property region** and **block** is visually identifiable and editable
- Simple properties (text, textarea, rich text) support **inline editing** directly on the rendered page
- Complex properties (media picker, content picker, etc.) open a **side panel** with the standard property editor
- Blocks can be **added, removed, reordered, and configured** visually
- The existing **form-based editor remains available** as a tab — the visual editor is an additional workspace view, not a replacement
- All changes flow through the **existing workspace data model** — save, publish, validation, and variants work identically

### Prior Art & Inspiration

| System | Approach | What We Learn |
|--------|----------|---------------|
| **BlockPreview (community)** | Server-side Razor rendering of individual blocks via API | Proves the rendering pipeline works; provides the foundation for partial re-render |
| **Umbraco Preview Mode** | Full-page iframe with cookie auth + SignalR refresh | Proves full-page rendering in backoffice iframe works |
| **WordPress Gutenberg** | Client-side block rendering with React | Good UX but tightly coupled to client-side rendering; Umbraco's Razor views are server-side |
| **Squarespace / Wix** | Full visual editor with drag-and-drop | Aspirational UX, but built from scratch with visual editing as the primary model |

### Design Principles

1. **Server-side rendering is the source of truth** — the visual editor shows exactly what the Razor views produce
2. **Automatic annotation** — text property output is automatically annotated via the Razor rendering pipeline; no developer template changes needed (validated in PoC)
3. **Editor-alias convention** — only text-oriented property editors (TextBox, TextArea, RichText, Markdown) are annotated; dropdowns, pickers, and booleans used in attributes/logic are skipped (prevents malformed HTML)
4. **No data model changes** — the visual editor is purely a UI concern; content storage, APIs, and validation are unchanged
5. **Server-injected guest script** — the interactive script is injected into the `<body>` by `UmbracoViewPage` during preview mode (with CSP nonce), not programmatically from the backoffice; this works cross-origin

---

## 2. Current State Analysis

### 2.1 Preview Mode

The existing preview system renders the full page in an iframe:

**Backend flow:**
1. `DocumentPreviewUrlController` generates a preview URL and calls `PreviewService.TryEnterPreviewAsync()` to set the `UMB_PREVIEW` cookie (HTTP-only, Secure, SameSite=None)
2. `PreviewAuthenticationMiddleware` (`src/Umbraco.Web.Common/Middleware/PreviewAuthenticationMiddleware.cs`) intercepts frontend requests, validates the cookie via `IPreviewTokenGenerator`, and adds the backoffice identity to the request principal
3. `UmbracoContext.InPreviewMode` (`src/Umbraco.Web.Common/UmbracoContext/UmbracoContext.cs:140-166`) detects preview mode by checking for the cookie on non-backoffice requests with an authenticated identity
4. The published cache serves draft/saved content when `InPreviewMode` is true

**Frontend flow:**
1. `preview.element.ts` (`src/Umbraco.Web.UI.Client/src/apps/preview/preview.element.ts`) renders an iframe with `sandbox="allow-scripts allow-same-origin allow-forms"`
2. `preview.context.ts` connects to `PreviewHub` via SignalR, listens for `refreshed(key)` events
3. When content matching the current document key is refreshed, the iframe URL is updated with a `rnd=` cache-buster to force reload
4. Preview apps (culture, device, segment, environment) are loaded via `<umb-extension-slot type="previewApp">`

**Key files:**
- `src/Umbraco.Core/Services/PreviewService.cs`
- `src/Umbraco.Core/Constants-Web.cs` — `PreviewCookieName = "UMB_PREVIEW"`
- `src/Umbraco.Web.Common/Middleware/PreviewAuthenticationMiddleware.cs`
- `src/Umbraco.Cms.Api.Management/Controllers/Document/DocumentPreviewUrlController.cs`
- `src/Umbraco.Cms.Api.Management/Preview/PreviewHub.cs`
- `src/Umbraco.Cms.Api.Management/Preview/PreviewHubUpdater.cs`
- `src/Umbraco.Web.UI.Client/src/apps/preview/preview.element.ts`
- `src/Umbraco.Web.UI.Client/src/packages/preview/context/preview.context.ts`

### 2.2 Block Editors

Block List and Block Grid store content as JSON (`BlockValue` with `ContentData`, `SettingsData`, `Layout`). The backoffice renders blocks via property editor UI components that support **custom views** — the `blockEditorCustomView` extension type.

**Rendering on the frontend site** uses HTML helper extensions:
- `GetBlockListHtmlAsync()` / `GetBlockGridHtmlAsync()` — iterate over blocks and render partial views by content type alias
- Partial views live in `Views/Partials/blocklist/` and `Views/Partials/blockgrid/`
- ViewComponents are also supported (matched by alias)

**Custom views in the backoffice** implement `UmbBlockEditorCustomViewElement` and receive reactive `content` and `settings` data via context consumption:
- `UMB_BLOCK_GRID_ENTRY_CONTEXT` / `UMB_BLOCK_LIST_ENTRY_CONTEXT`
- `UMB_BLOCK_GRID_MANAGER_CONTEXT` / `UMB_BLOCK_LIST_MANAGER_CONTEXT`

**Key files:**
- `src/Umbraco.Core/Models/Blocks/` — `BlockValue`, `BlockListValue`, `BlockGridValue`, `BlockItemData`
- `src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs`
- `src/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensions.cs`
- `src/Umbraco.Web.UI.Client/src/packages/block/block-custom-view/`

### 2.3 BlockPreview Package (Community)

BlockPreview (`D:\Community\BlockPreview-v5`) proves the core rendering pattern:

1. **API endpoints** (`POST /block-preview/api/v1.0/preview/{grid|list|rte}`) accept raw block JSON + context parameters
2. **BlockDataConverter** deserializes JSON → `BlockItemData`, converts properties via `editor.GetValueEditor().FromEditor()`, then calls `BlockEditorConverter.ConvertToElement()` to create `IPublishedElement` instances
3. **BlockPreviewService** resolves the parent `IPublishedContent` (from published cache, blueprint cache, or by content type), sets up `IPublishedRequest` via `IPublishedRouter.CreateRequestAsync()`, and creates a `ViewDataDictionary` with the typed block model
4. **BlockViewRenderer** tries ViewComponent first (by PascalCase/camelCase alias via `IViewComponentSelector`), falls back to partial view via `IRazorViewEngine.FindView()`
5. **Markup cleanup** via HtmlAgilityPack: all `<a href>` set to `javascript:;`, all form elements disabled
6. **Enrichment hooks**: `IBlockPreviewRequestEnricher` (pre-render) and `IBlockPreviewResponseEnricher` (post-render)
7. **Frontend** uses Lit custom view elements that observe block data changes, debounce 500ms, then call the API with a concurrency queue (max 3 concurrent requests)

**Key limitation**: BlockPreview renders **individual blocks** in isolation. It does not render the full page with master template context.

### 2.4 Content Editing Workspace

The document workspace (`UmbDocumentWorkspaceContext`) manages content state:

- `UmbContentWorkspaceDataManager` tracks current vs persisted property values per variant
- Property editors communicate changes via `UmbChangeEvent` → `UmbPropertyDatasetContext` → workspace context
- Save/publish actions (`UmbSaveWorkspaceAction`, `UmbDocumentSaveAndPublishWorkspaceAction`) go through `validateAndSubmit()` pipeline
- Workspace views are registered as manifests with `type: 'workspaceView'` and conditions

**Key files:**
- `src/Umbraco.Web.UI.Client/src/packages/documents/documents/workspace/document-workspace.context.ts`
- `src/Umbraco.Web.UI.Client/src/packages/content/content/workspace/content-detail-workspace-base.ts`
- `src/Umbraco.Web.UI.Client/src/packages/content/content/workspace/views/edit/content-editor.element.ts`

### 2.5 Existing Communication Patterns

The backoffice already uses `postMessage` for cross-window communication in the auth flow (`src/Umbraco.Web.UI.Client/src/packages/core/auth/auth.context.ts:267-310`):
- Origin-checked messages between parent and popup windows
- `BroadcastChannel` for cross-tab session sync
- Timeout fallbacks and state matching for security

This establishes the pattern for iframe ↔ backoffice communication.

### 2.6 TagHelper Infrastructure

Currently minimal — only `CspNonceTagHelper` exists (`src/Umbraco.Web.Common/TagHelpers/CspNonceTagHelper.cs`). No existing TagHelper infrastructure for content annotation. This means TagHelpers for visual editing are a greenfield addition.

---

## 3. Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│  Backoffice Shell (Lit Web Components)                       │
│                                                              │
│  ┌──────────────────────────────────────┐  ┌──────────────┐ │
│  │  Visual Editor Workspace View        │  │ Property     │ │
│  │  ┌────────────────────────────────┐  │  │ Panel        │ │
│  │  │  Iframe (full rendered page)   │  │  │              │ │
│  │  │  ┌──────────────────────────┐  │  │  │ [TextBox]    │ │
│  │  │  │ Annotated regions       ←──────────── Edits flow │ │
│  │  │  │ data-umb-property="..."  │  │  │  │ back to      │ │
│  │  │  │ data-umb-block="..."     │  │  │  │ workspace    │ │
│  │  │  └──────────────────────────┘  │  │  │ context      │ │
│  │  └────────────────────────────────┘  │  │              │ │
│  │  ┌────────────────────────────────┐  │  │ [MediaPicker]│ │
│  │  │  Overlay Layer                 │  │  │              │ │
│  │  │  - Selection outlines          │  │  │ [BlockConfig]│ │
│  │  │  - Hover highlights            │  │  │              │ │
│  │  │  - Add/remove block buttons    │  │  └──────────────┘ │
│  │  │  - Drag handles                │  │                    │
│  │  └────────────────────────────────┘  │                    │
│  └──────────────────────────────────────┘                    │
│  ┌──────────────────────────────────────────────────────────┐│
│  │  Workspace Footer: [Save] [Publish] [Exit Visual]       ││
│  └──────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘

Communication:
  Iframe ──postMessage──→ Backoffice Shell
  Backoffice ──postMessage──→ Iframe (injected script)
  Backoffice ──REST API──→ Partial Re-render Endpoint
  Server ──SignalR──→ Backoffice (content refresh notifications)
```

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Annotated Rendering** (Server, Razor) | Emit `data-umb-*` attributes around editable regions when in visual edit mode |
| **Iframe Guest Script** (JS, injected into rendered page) | Discover annotated regions, handle hover/click/inline-edit, communicate with parent via postMessage |
| **Visual Editor View** (Lit, backoffice) | Host iframe + overlay, manage selection state, render property panel, handle block operations |
| **Partial Re-render API** (Server, Management API) | Accept property values + context, render a single region/block server-side, return HTML |
| **Workspace Integration** (Lit, backoffice) | Same `UmbDocumentWorkspaceContext`, same save/publish/validate pipeline |

---

## 4. Component Design

### 4.1 Annotated Rendering Pipeline

#### 4.1.1 Visual Edit Mode Detection

Add a new mode alongside preview, detected via a separate cookie or query parameter:

```csharp
// New property on UmbracoContext (or a new service)
public bool InVisualEditMode { get; }
```

**Detection strategy**: Similar to `InPreviewMode` but with a distinct cookie (`UMB_VISUAL_EDIT`) to allow independent state. The visual editor initiates this mode when opening the iframe.

**Alternative**: Reuse `InPreviewMode` with an additional query parameter (`?visualEdit=true`) that sets an `HttpContext.Items` flag. This avoids a second cookie but couples the two features.

**Recommendation**: Separate cookie. Visual edit mode has different security implications (the page needs to accept injected scripts and respond to postMessage).

#### 4.1.2 Automatic Block Annotation

The `GetBlockListHtmlAsync` and `GetBlockGridHtmlAsync` extension methods already control the rendering loop for each block. In visual edit mode, they can **automatically wrap each block's output** with annotation attributes.

**No developer action required** — blocks get annotated for free.

```csharp
// In BlockListTemplateExtensions, wrapping each block's partial view output:
if (umbracoContext.InVisualEditMode)
{
    writer.Write($"<div data-umb-block=\"{index}\" " +
        $"data-umb-block-key=\"{block.ContentKey}\" " +
        $"data-umb-content-type=\"{block.Content.ContentType.Alias}\" " +
        $"data-umb-property=\"{propertyAlias}\" " +
        $"data-umb-editor=\"Umbraco.BlockList\">");
}

await html.RenderPartialAsync(partialViewName, block);

if (umbracoContext.InVisualEditMode)
{
    writer.Write("</div>");
}
```

For Block Grid, the existing grid layout divs would gain additional attributes:

```html
<!-- Existing grid item wrapper gains annotation attributes -->
<div class="umb-block-grid__layout-item"
     data-umb-block="0"
     data-umb-block-key="abc-123"
     data-umb-content-type="heroBlock"
     data-umb-property="blockGrid"
     data-umb-editor="Umbraco.BlockGrid"
     data-umb-row-span="1"
     data-umb-col-span="12">
  <!-- Block's partial view output -->
</div>
```

**Block Grid areas** also get annotated as drop zones:

```html
<div class="umb-block-grid__area"
     data-umb-block-area="sidebarArea"
     data-umb-parent-block-key="abc-123"
     data-umb-property="blockGrid">
  <!-- Blocks within this area -->
</div>
```

**Implementation location**: Modify the partial view rendering in:
- `src/Umbraco.Web.Common/Extensions/BlockListTemplateExtensions.cs`
- `src/Umbraco.Web.Common/Extensions/BlockGridTemplateExtensions.cs`
- The default block grid partial views in `src/Umbraco.Web.UI/Views/Partials/blockgrid/`

#### 4.1.3 Property-Level Annotation (Opt-in)

Individual properties (title, body text, images) require explicit annotation because the developer controls the markup. Two complementary approaches:

**A. TagHelper (recommended for Razor views):**

```csharp
// New TagHelper: <umb-visual-region>
[HtmlTargetElement("umb-visual-region")]
public class VisualEditorRegionTagHelper : TagHelper
{
    [HtmlAttributeName("property")]
    public string PropertyAlias { get; set; }

    [HtmlAttributeName("content")]
    public IPublishedContent Content { get; set; }

    [HtmlAttributeName("editor")]
    public string? EditorAlias { get; set; }  // Auto-detected if omitted

    [HtmlAttributeName("inline")]
    public bool AllowInlineEdit { get; set; } = false;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (_umbracoContext.InVisualEditMode)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("data-umb-property", PropertyAlias);
            output.Attributes.SetAttribute("data-umb-content-key", Content.Key.ToString());
            output.Attributes.SetAttribute("data-umb-editor", EditorAlias ?? ResolveEditorAlias());

            if (AllowInlineEdit)
                output.Attributes.SetAttribute("data-umb-inline-edit", "true");
        }
        else
        {
            output.TagName = null; // Renders children only, no wrapper element
        }
    }
}
```

**Usage in developer's Razor view:**

```html
<!-- Before (unchanged, still works) -->
<h1>@Model.Value("pageTitle")</h1>

<!-- After (opt-in visual editing) -->
<umb-visual-region property="pageTitle" content="@Model" inline="true">
    <h1>@Model.Value("pageTitle")</h1>
</umb-visual-region>

<!-- For media/complex properties -->
<umb-visual-region property="heroImage" content="@Model">
    <img src="@Model.Value<MediaWithCrops>("heroImage")?.GetCropUrl(width: 1920)" />
</umb-visual-region>
```

**B. HtmlHelper extension (alternative):**

```csharp
@using (Html.BeginVisualEditorRegion("pageTitle", Model, inline: true))
{
    <h1>@Model.Value("pageTitle")</h1>
}
```

**Recommendation**: Provide both, but promote the TagHelper as the primary API. It's more readable and aligns with modern ASP.NET Core patterns.

#### 4.1.4 Script Injection

When `InVisualEditMode` is true, inject a small JavaScript file into the page that:
- Discovers all `data-umb-*` annotated elements
- Sets up the postMessage communication channel
- Handles hover highlights, click selection, and inline editing

This can be injected via a TagHelper on `<body>` or via middleware that appends a `<script>` tag before `</body>`.

```csharp
// Middleware approach: append script before </body>
public class VisualEditorScriptInjectionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsVisualEditMode(context))
        {
            await _next(context);
            return;
        }

        // Buffer the response, inject script before </body>
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        buffer.Seek(0, SeekOrigin.Begin);
        var html = await new StreamReader(buffer).ReadToEndAsync();

        var script = "<script src=\"/umbraco/visual-editor/guest.js\"></script>";
        html = html.Replace("</body>", $"{script}</body>");

        context.Response.Body = originalBody;
        await context.Response.WriteAsync(html);
    }
}
```

**Alternative**: Use a `<script>` TagHelper on `<html>` or `<body>` that auto-injects in visual edit mode.

### 4.2 Visual Editor Frontend

#### 4.2.1 Workspace View Registration

Register as a new workspace view tab alongside "Content" and "Info":

```typescript
// manifests.ts
{
    type: 'workspaceView',
    alias: 'Umb.WorkspaceView.Document.VisualEditor',
    name: 'Document Workspace Visual Editor View',
    element: () => import('./visual-editor.element.js'),
    weight: 300,  // Higher than Content (200) = appears first
    meta: {
        label: '#visualEditor_tabLabel',  // Localized
        pathname: 'visual',
        icon: 'icon-eye',
    },
    conditions: [
        {
            alias: UMB_WORKSPACE_CONDITION_ALIAS,
            match: UMB_DOCUMENT_WORKSPACE_ALIAS,
        },
        // Only show for documents that have a template assigned
        {
            alias: 'Umb.Condition.Document.HasTemplate',
        },
        // Only show for existing (saved) documents
        {
            alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
            match: false,
        },
    ],
}
```

#### 4.2.2 Visual Editor Element

The main element hosts the iframe and the overlay/panel system:

```typescript
@customElement('umb-visual-editor')
export class UmbVisualEditorElement extends UmbLitElement {
    // Consume the existing document workspace context
    #workspaceContext?: UmbDocumentWorkspaceContext;

    // Visual editor state
    @state() private _selectedRegion?: VisualEditorRegion;
    @state() private _regions: VisualEditorRegion[] = [];
    @state() private _iframeReady = false;
    @state() private _pageUrl?: string;

    // Iframe reference
    @query('iframe') private _iframe!: HTMLIFrameElement;

    constructor() {
        super();
        this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (ctx) => {
            this.#workspaceContext = ctx;
        });
    }

    override render() {
        return html`
            <div id="visual-editor-container">
                <div id="iframe-wrapper">
                    <iframe
                        src=${ifDefined(this._pageUrl)}
                        title="Visual Editor"
                        @load=${this.#onIframeLoad}
                        sandbox="allow-scripts allow-same-origin allow-forms">
                    </iframe>
                    <umb-visual-editor-overlay
                        .regions=${this._regions}
                        .selectedRegion=${this._selectedRegion}
                        .iframeElement=${this._iframe}
                        @region-selected=${this.#onRegionSelected}
                        @block-add=${this.#onBlockAdd}
                        @block-remove=${this.#onBlockRemove}
                        @block-reorder=${this.#onBlockReorder}>
                    </umb-visual-editor-overlay>
                </div>
                ${this._selectedRegion ? html`
                    <umb-visual-editor-panel
                        .region=${this._selectedRegion}
                        .workspaceContext=${this.#workspaceContext}
                        @property-changed=${this.#onPropertyChanged}>
                    </umb-visual-editor-panel>
                ` : nothing}
            </div>
        `;
    }
}
```

#### 4.2.3 Overlay System

A transparent layer positioned over the iframe that intercepts pointer events and renders selection/hover UI. It does NOT live inside the iframe (avoiding cross-origin issues).

```typescript
@customElement('umb-visual-editor-overlay')
export class UmbVisualEditorOverlayElement extends UmbLitElement {
    @property({ attribute: false }) regions: VisualEditorRegion[] = [];
    @property({ attribute: false }) selectedRegion?: VisualEditorRegion;
    @property({ attribute: false }) iframeElement?: HTMLIFrameElement;

    // Overlay renders absolutely-positioned outlines based on region coordinates
    // reported by the iframe guest script via postMessage

    override render() {
        return html`
            ${this.regions.map(region => html`
                <div class="region-outline"
                     style=${this.#getRegionStyle(region)}
                     @click=${() => this.#selectRegion(region)}
                     @mouseenter=${() => this.#hoverRegion(region)}
                     @mouseleave=${() => this.#unhoverRegion()}>
                    ${region.type === 'block' ? html`
                        <div class="block-toolbar">
                            <button @click=${() => this.#editBlock(region)}>Edit</button>
                            <button @click=${() => this.#removeBlock(region)}>Remove</button>
                            <span class="drag-handle">Drag</span>
                        </div>
                    ` : nothing}
                </div>
            `)}
            ${this.#renderDropZones()}
        `;
    }
}
```

**Coordinate synchronization**: The overlay must match iframe content positions. The guest script reports element bounding rects, and the overlay translates them accounting for iframe offset and scroll position. The guest script sends updated coordinates on scroll, resize, and mutation events.

#### 4.2.4 Property Panel

When a region is selected, the right panel shows the appropriate property editor(s):

- **For a property region**: Shows the single property editor (text box, media picker, etc.)
- **For a block region**: Shows all property editors for the block's content type (and settings)

The panel reuses existing `<umb-property>` components, binding their values to the workspace context. This means standard validation, configuration, and behavior are inherited.

```typescript
@customElement('umb-visual-editor-panel')
export class UmbVisualEditorPanelElement extends UmbLitElement {
    @property({ attribute: false }) region?: VisualEditorRegion;
    @property({ attribute: false }) workspaceContext?: UmbDocumentWorkspaceContext;

    override render() {
        if (!this.region) return nothing;

        if (this.region.type === 'property') {
            return html`
                <div class="panel-header">
                    <h4>${this.region.label}</h4>
                    <button @click=${this.#close}>Close</button>
                </div>
                <umb-property
                    alias=${this.region.propertyAlias}
                    .value=${this.region.currentValue}
                    @change=${this.#onPropertyChange}>
                </umb-property>
            `;
        }

        if (this.region.type === 'block') {
            // Render all properties for this block's content type
            return html`
                <div class="panel-header">
                    <h4>${this.region.contentTypeAlias}</h4>
                    <button @click=${this.#close}>Close</button>
                </div>
                ${this.region.properties.map(prop => html`
                    <umb-property
                        alias=${prop.alias}
                        .value=${prop.value}
                        @change=${(e) => this.#onBlockPropertyChange(prop.alias, e)}>
                    </umb-property>
                `)}
            `;
        }
    }
}
```

### 4.3 Iframe Communication Protocol

All communication between the backoffice shell and the rendered page iframe uses `window.postMessage` with origin checking. This works even when the backoffice and frontend are on different origins.

#### 4.3.1 Message Types

**Iframe → Parent (guest script → visual editor):**

```typescript
interface VisualEditorMessage {
    type: string;
    source: 'umb-visual-editor-guest';  // Identifies our messages
}

// Sent on load after discovering all annotated regions
interface RegionMapMessage extends VisualEditorMessage {
    type: 'umb:visual-editor:region-map';
    regions: RegionDescriptor[];
}

interface RegionDescriptor {
    id: string;                    // Unique ID for this region instance
    type: 'property' | 'block' | 'block-area';
    propertyAlias: string;
    contentKey: string;
    editorAlias?: string;          // e.g., "Umbraco.TextBox"
    contentTypeAlias?: string;     // For blocks
    blockKey?: string;             // For blocks
    blockIndex?: number;           // For blocks
    areaAlias?: string;            // For block grid areas
    parentBlockKey?: string;       // For blocks in areas
    inlineEditable: boolean;       // Whether contenteditable is supported
    boundingRect: DOMRect;         // Position for overlay
}

// Sent on hover/scroll/resize with updated positions
interface RegionUpdateMessage extends VisualEditorMessage {
    type: 'umb:visual-editor:region-update';
    regions: { id: string; boundingRect: DOMRect }[];
}

// Sent when user clicks an annotated region
interface RegionSelectedMessage extends VisualEditorMessage {
    type: 'umb:visual-editor:region-selected';
    regionId: string;
}

// Sent when inline text editing produces a change
interface InlineEditMessage extends VisualEditorMessage {
    type: 'umb:visual-editor:inline-edit';
    regionId: string;
    propertyAlias: string;
    contentKey: string;
    value: string;                 // New text content
}
```

**Parent → Iframe (visual editor → guest script):**

```typescript
// Highlight a specific region (on panel hover, etc.)
interface HighlightRegionMessage {
    type: 'umb:visual-editor:highlight-region';
    regionId: string;
}

// Clear highlight
interface ClearHighlightMessage {
    type: 'umb:visual-editor:clear-highlight';
}

// Enable inline editing on a region
interface EnableInlineEditMessage {
    type: 'umb:visual-editor:enable-inline-edit';
    regionId: string;
}

// Update rendered HTML for a specific region (after re-render)
interface UpdateRegionHtmlMessage {
    type: 'umb:visual-editor:update-region-html';
    regionId: string;
    html: string;
}

// Request fresh region coordinates (after DOM changes)
interface RequestRegionUpdateMessage {
    type: 'umb:visual-editor:request-region-update';
}
```

#### 4.3.2 Guest Script

A small JavaScript file injected into the rendered page when in visual edit mode:

```javascript
// /umbraco/visual-editor/guest.js (served as static asset)
(function() {
    'use strict';

    const PARENT_ORIGIN = document.referrer
        ? new URL(document.referrer).origin
        : window.location.origin;

    function send(message) {
        window.parent.postMessage(
            { ...message, source: 'umb-visual-editor-guest' },
            PARENT_ORIGIN
        );
    }

    // Discover all annotated regions
    function discoverRegions() {
        const regions = [];
        document.querySelectorAll('[data-umb-property], [data-umb-block], [data-umb-block-area]').forEach((el, i) => {
            regions.push({
                id: el.dataset.umbRegionId || `region-${i}`,
                type: el.dataset.umbBlock !== undefined ? 'block'
                    : el.dataset.umbBlockArea !== undefined ? 'block-area'
                    : 'property',
                propertyAlias: el.dataset.umbProperty,
                contentKey: el.dataset.umbContentKey,
                editorAlias: el.dataset.umbEditor,
                contentTypeAlias: el.dataset.umbContentType,
                blockKey: el.dataset.umbBlockKey,
                blockIndex: parseInt(el.dataset.umbBlock) || undefined,
                areaAlias: el.dataset.umbBlockArea,
                parentBlockKey: el.dataset.umbParentBlockKey,
                inlineEditable: el.dataset.umbInlineEdit === 'true',
                boundingRect: el.getBoundingClientRect(),
            });
            el.dataset.umbRegionId = regions[regions.length - 1].id;
        });
        return regions;
    }

    // Send region map on load
    window.addEventListener('load', () => {
        send({ type: 'umb:visual-editor:region-map', regions: discoverRegions() });
    });

    // Update positions on scroll/resize
    const updatePositions = debounce(() => {
        const updates = [];
        document.querySelectorAll('[data-umb-region-id]').forEach(el => {
            updates.push({ id: el.dataset.umbRegionId, boundingRect: el.getBoundingClientRect() });
        });
        send({ type: 'umb:visual-editor:region-update', regions: updates });
    }, 16); // ~60fps

    window.addEventListener('scroll', updatePositions, { passive: true });
    window.addEventListener('resize', updatePositions);
    new MutationObserver(updatePositions).observe(document.body, { childList: true, subtree: true });

    // Handle clicks on regions
    document.addEventListener('click', (e) => {
        const region = e.target.closest('[data-umb-region-id]');
        if (region) {
            e.preventDefault();
            e.stopPropagation();
            send({ type: 'umb:visual-editor:region-selected', regionId: region.dataset.umbRegionId });
        }
    }, true);

    // Listen for parent messages
    window.addEventListener('message', (evt) => {
        if (evt.origin !== PARENT_ORIGIN) return;
        if (!evt.data?.type?.startsWith('umb:visual-editor:')) return;

        switch (evt.data.type) {
            case 'umb:visual-editor:highlight-region':
                highlightRegion(evt.data.regionId);
                break;
            case 'umb:visual-editor:clear-highlight':
                clearHighlight();
                break;
            case 'umb:visual-editor:enable-inline-edit':
                enableInlineEdit(evt.data.regionId);
                break;
            case 'umb:visual-editor:update-region-html':
                updateRegionHtml(evt.data.regionId, evt.data.html);
                break;
            case 'umb:visual-editor:request-region-update':
                send({ type: 'umb:visual-editor:region-map', regions: discoverRegions() });
                break;
        }
    });

    // Inline editing support
    function enableInlineEdit(regionId) {
        const el = document.querySelector(`[data-umb-region-id="${regionId}"]`);
        if (!el || el.dataset.umbInlineEdit !== 'true') return;

        el.contentEditable = 'true';
        el.focus();

        el.addEventListener('input', debounce(() => {
            send({
                type: 'umb:visual-editor:inline-edit',
                regionId,
                propertyAlias: el.dataset.umbProperty,
                contentKey: el.dataset.umbContentKey,
                value: el.textContent,  // or innerHTML for rich text
            });
        }, 300));

        el.addEventListener('blur', () => {
            el.contentEditable = 'false';
            // Trigger re-render to get server-rendered version
            send({
                type: 'umb:visual-editor:inline-edit-complete',
                regionId,
                propertyAlias: el.dataset.umbProperty,
                contentKey: el.dataset.umbContentKey,
                value: el.textContent,
            });
        });
    }

    function debounce(fn, ms) {
        let timer;
        return (...args) => { clearTimeout(timer); timer = setTimeout(() => fn(...args), ms); };
    }
})();
```

#### 4.3.3 Security Considerations

- **Origin checking**: Both sides verify `evt.origin` before processing messages
- **Message namespace**: All messages prefixed with `umb:visual-editor:` and include `source: 'umb-visual-editor-guest'`
- **No sensitive data in messages**: Only region IDs, property aliases, and text values — no tokens or credentials
- **Iframe sandbox**: `allow-scripts allow-same-origin allow-forms` (same as current preview)
- **Visual edit cookie**: Same security attributes as preview cookie (HTTP-only, Secure, SameSite=None)

### 4.4 Partial Re-render API

When a property value changes, the affected region needs to be re-rendered server-side. This extends the pattern established by BlockPreview.

#### 4.4.1 Endpoint Design

```
POST /umbraco/management/api/v1/visual-editor/render-region
Authorization: BackOfficeAccess policy
Content-Type: application/json
```

**Request body:**

```json
{
    "contentKey": "abc-123-...",
    "propertyAlias": "contentBlocks",
    "culture": "en-US",
    "segment": null,
    "regionType": "block",
    "blockKey": "def-456-...",
    "propertyValues": {
        "pageTitle": "Updated Title",
        "contentBlocks": "{ ... block JSON ... }"
    }
}
```

**Response:**

```json
{
    "html": "<div data-umb-block=\"0\" ...>...rendered HTML...</div>",
    "regionId": "region-3"
}
```

#### 4.4.2 Rendering Strategy

The endpoint needs to render a Razor partial view with **unsaved property values**. This extends BlockPreview's approach:

1. **Resolve the content item** from published cache (or create temporary published content from the provided values)
2. **Override property values** with the provided `propertyValues` — create a wrapper `IPublishedContent` that returns the new values instead of cached ones
3. **Set up the rendering context** — `IPublishedRequest`, culture, template
4. **Render the specific region** — for blocks, use the block partial view; for properties, render the annotated TagHelper wrapper with updated content
5. **Clean up markup** — same sanitization as BlockPreview (disable links/forms)
6. **Return HTML** — the guest script replaces the region's DOM content

```csharp
[HttpPost("render-region")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public async Task<IActionResult> RenderRegion([FromBody] RenderRegionRequest request)
{
    // 1. Resolve base content
    var content = await _publishedContentCache.GetByIdAsync(request.ContentKey);

    // 2. Create property-overridden wrapper
    var overriddenContent = new PropertyOverridePublishedContent(content, request.PropertyValues);

    // 3. Set up published request context
    var publishedRequest = await _publishedRouter.CreateRequestAsync(/* ... */);

    // 4. Render based on region type
    string html;
    if (request.RegionType == "block")
    {
        html = await _blockRenderer.RenderBlockAsync(
            overriddenContent, request.PropertyAlias, request.BlockKey, request.Culture);
    }
    else
    {
        html = await _propertyRenderer.RenderPropertyAsync(
            overriddenContent, request.PropertyAlias, request.Culture);
    }

    // 5. Clean up
    html = MarkupSanitizer.CleanUp(html);

    return Ok(new { html, regionId = request.RegionId });
}
```

#### 4.4.3 PropertyOverridePublishedContent

A decorator around `IPublishedContent` that intercepts property access and returns overridden values:

```csharp
public class PropertyOverridePublishedContent : IPublishedContent
{
    private readonly IPublishedContent _inner;
    private readonly Dictionary<string, object?> _overrides;

    public PropertyOverridePublishedContent(
        IPublishedContent inner,
        Dictionary<string, object?> overrides)
    {
        _inner = inner;
        _overrides = overrides;
    }

    public IPublishedProperty? GetProperty(string alias)
    {
        if (_overrides.TryGetValue(alias, out var value))
        {
            var innerProp = _inner.GetProperty(alias);
            return new OverriddenPublishedProperty(innerProp, value);
        }
        return _inner.GetProperty(alias);
    }

    // Delegate all other IPublishedContent members to _inner
}
```

#### 4.4.4 Performance

- **Debounce**: Frontend debounces property changes (500ms) before requesting re-render
- **Concurrency queue**: Max 3 concurrent render requests (same as BlockPreview)
- **Optimistic updates**: For inline text editing, show the user's typed text immediately; replace with server-rendered HTML on blur or after debounce
- **Partial DOM replacement**: Only the changed region's HTML is swapped, not the full page
- **Caching**: View resolution paths can be cached (same as BlockPreview's `BlockPreviewViewResolver`)

### 4.5 Inline Editing

#### 4.5.1 Supported Property Types

| Property Editor | Inline Strategy | Notes |
|----------------|-----------------|-------|
| `Umbraco.TextBox` | `contenteditable` on the element | Plain text only |
| `Umbraco.TextArea` | `contenteditable` on the element | Preserve line breaks |
| `Umbraco.TinyMCE` / `Umbraco.RichText` | `contenteditable` with basic formatting | Complex — may need TipTap integration |
| `Umbraco.MediaPicker3` | Click → panel | No inline editing |
| `Umbraco.ContentPicker` | Click → panel | No inline editing |
| `Umbraco.BlockList` | Block manipulation (add/remove/reorder) | See section 4.6 |
| `Umbraco.BlockGrid` | Block manipulation + area support | See section 4.6 |
| All others | Click → panel | Falls back to standard property editor |

#### 4.5.2 Inline Text Editing Flow

1. User clicks an annotated text property region
2. Visual editor sends `enable-inline-edit` message to guest script
3. Guest script sets `contentEditable="true"` on the element, focuses it
4. User types — guest script debounces (300ms) and sends `inline-edit` messages with new text
5. Visual editor receives the value, updates the workspace context data model
6. On blur, guest script sends `inline-edit-complete`
7. Visual editor triggers a partial re-render to get the server-rendered version (handles any Razor formatting, character limits, etc.)
8. Re-rendered HTML replaces the region via `update-region-html` message

**Important**: The workspace data model is the source of truth. Inline edits are immediately reflected in the workspace context so that:
- Switching to the Content tab shows the updated value
- Save/publish includes the inline changes
- Validation runs against the new value

#### 4.5.3 Rich Text Inline Editing

Rich text is significantly more complex than plain text. Two approaches:

**A. Basic `contenteditable` (Phase 3)**
- Allow basic text editing and formatting via browser's built-in contenteditable
- Extract HTML content and send to workspace
- Server re-render normalizes the output
- Limitation: No Umbraco-specific features (media insertion, macros, etc.)

**B. Embedded TipTap (Future)**
- Load TipTap editor inline within the annotated region
- Full rich text editing capabilities
- Requires significant integration work — the TipTap editor needs Umbraco's toolbar and plugin system
- Consider this a stretch goal

**Recommendation**: Phase 3 uses basic contenteditable. Embedded TipTap is a separate future initiative.

### 4.6 Block Manipulation

#### 4.6.1 Block Selection & Editing

When a user clicks a block region:
1. The overlay shows a selection outline with a toolbar (Edit, Settings, Delete, Drag Handle)
2. The property panel shows the block's content properties
3. A "Settings" toggle in the panel switches to the block's settings properties
4. Changes to any property trigger a partial re-render of that block

#### 4.6.2 Adding Blocks

**Between blocks**: The overlay shows "+" insertion points between blocks and at the end of a block list/area. Clicking "+" opens the existing block type picker modal.

```typescript
async #onBlockAdd(event: CustomEvent<{ propertyAlias: string; insertIndex: number; areaAlias?: string }>) {
    const { propertyAlias, insertIndex, areaAlias } = event.detail;

    // Open existing block type picker
    const result = await this.#openBlockTypePicker(propertyAlias);
    if (!result) return;

    // Create new block via workspace context
    const blockManager = this.#getBlockManager(propertyAlias);
    await blockManager.create(result.contentTypeKey, insertIndex, areaAlias);

    // Trigger full page re-render (new block needs server rendering)
    await this.#reloadIframe();
}
```

**In block grid areas**: Drop zones within areas show "+" buttons when empty.

#### 4.6.3 Removing Blocks

The block toolbar's "Delete" button triggers a confirmation dialog, then removes the block from the workspace data model and triggers a re-render.

#### 4.6.4 Reordering Blocks

The overlay renders drag handles on each block. Dragging uses the existing `UmbSorterController` patterns:
- `dragstart` sets DataTransfer data with block identifier
- `dragover` on other blocks/drop zones shows insertion indicators
- `drop` updates the block order in the workspace data model
- Triggers re-render of the affected block list/grid region

**For Block Grid**: Reordering also supports moving blocks between areas. The overlay shows valid drop zones based on the block type's configuration (allowAtRoot, allowInAreas).

### 4.7 Workspace Integration

#### 4.7.1 Shared State

The visual editor workspace view **shares the same `UmbDocumentWorkspaceContext`** as the Content tab. This means:

- Property values are always in sync between views
- Switching between "Content" and "Visual" tabs reflects the same data
- Save/publish actions work identically
- Validation state is shared
- Undo/redo (if implemented) applies to both views

#### 4.7.2 Visual Editor → Workspace Data Flow

```
User edits in visual editor
  → postMessage to backoffice (inline edit) or panel property change
  → Visual editor updates workspace context:
      workspaceContext.propertyValueByAlias(alias).setValue(newValue)
  → Workspace context marks variant as dirty
  → Triggers partial re-render API call
  → Updated HTML sent back to iframe via postMessage
```

#### 4.7.3 Workspace → Visual Editor Data Flow

```
User edits in Content tab, then switches to Visual tab
  → Visual editor reads current values from workspace context
  → Initiates iframe load with preview URL
  → Page renders with current (saved) content
  → For unsaved changes: visual editor sends property overrides to re-render API
  → Updated regions are patched into the iframe
```

**Challenge**: The iframe renders the **saved** version of the page. Unsaved changes need to be overlaid via re-render API calls for each dirty property. This is acceptable for a few changed properties but may be slow if many properties are dirty.

**Optimization**: If the document has unsaved changes when entering the visual editor, prompt the user to save first (or auto-save as draft).

#### 4.7.4 Save & Preview Integration

The existing "Save and Preview" action can be extended with a "Save and Visual Edit" variant that:
1. Saves the document
2. Opens/refreshes the visual editor iframe (instead of a new preview window)

#### 4.7.5 Conditions

The visual editor tab should only appear when:
- The document has a template assigned
- The document has been saved at least once (has a unique key)
- The user has edit permissions

---

## 5. Data Flow

### 5.1 Initial Load (validated in PoC)

```
1. User opens document workspace, clicks "Visual" tab
2. Visual editor calls UmbPreviewRepository.getPreviewUrl() — this sets the UMB_PREVIEW cookie
3. Visual editor builds iframe URL: new URL(documentUnique, serverUrl)
   (NOT the preview app URL returned by the API — that's the backoffice preview route)
4. Iframe loads https://server:port/{content-key-guid}?rnd=...
5. PreviewAuthenticationMiddleware detects preview cookie:
   a. Adds backoffice identity to request principal
   b. Calls VisualEditorPropertyTracker.Enable() for this async context
6. Razor renders the page:
   a. PublishedContentExtensions.Value<T>() checks editor alias → records text property accesses
   b. UmbracoViewPage.Write(string?) consumes tracked access → wraps with <span data-umb-property>
   c. UmbracoViewPage.BeginWriteAttribute() clears tracker → attribute values never annotated
   d. Block list/grid partials emit data-umb-block-key / data-element-key on block wrappers
   e. UmbracoViewPage.WriteUmbracoContent() injects guest script into <body> with CSP nonce
7. Guest script discovers annotated regions, sends region-map to parent via postMessage
8. Visual editor receives region-map, enables hover/click interactions
9. SignalR connection to PreviewHub established for live refresh after save
```

### 5.2 Property Edit (Panel)

```
1. User clicks a property region in iframe
2. Guest script sends region-selected message
3. Visual editor shows property panel with appropriate editor
4. User changes value in property editor
5. Property editor emits UmbChangeEvent
6. Visual editor updates workspace context value
7. Visual editor calls POST /visual-editor/render-region with new value
8. Server renders the region with overridden value
9. Visual editor sends update-region-html to guest script
10. Guest script replaces DOM content, re-reports region positions
```

### 5.3 Inline Text Edit

```
1. User double-clicks an inline-editable text region
2. Visual editor sends enable-inline-edit to guest script
3. Guest script sets contentEditable, user types
4. Guest script debounces (300ms) and sends inline-edit messages
5. Visual editor updates workspace context value (immediate)
6. On blur: guest script sends inline-edit-complete
7. Visual editor calls render-region API for server-rendered version
8. Server HTML replaces the contenteditable content
```

### 5.4 Block Add/Remove/Reorder

```
1. User clicks "+" between blocks or drags a block
2. Overlay handles the interaction
3. Visual editor updates block data in workspace context:
   - BlockManager.create() / remove() / reorder()
4. Full re-render of the block list/grid region via render-region API
   (or full iframe reload for complex changes)
5. Guest script re-discovers regions, sends updated region-map
```

### 5.5 Save/Publish

```
1. User clicks Save or Publish in workspace footer
2. Same flow as Content tab:
   - Variant selection (if multi-culture)
   - Validation
   - API call to save/publish
3. On success: iframe reloads (or SignalR triggers refresh)
4. Guest script re-discovers regions
```

---

## 6. Phased Delivery

### Phase 1: Click-to-Edit with Automatic Annotation (PoC COMPLETE)

**Goal**: Show the full rendered page in a workspace view tab with automatic property and block annotations. Clicking a property or block opens a side panel. Zero template changes required.

**What was built and validated:**
- Visual editor workspace view (tab between Content and Info)
- Iframe loading the actual Umbraco-rendered page via preview cookie auth
- **Automatic property annotation** via `VisualEditorPropertyTracker` in the Razor pipeline — text properties (`@Model.Title`, `@Model.Value("title")`, ModelsBuilder properties) wrapped with `<span data-umb-property>` at render time
- **Editor-alias filtering** — only TextBox, TextArea, RichText, and MarkdownEditor properties annotated; attribute-context properties (dropdowns, pickers) skipped
- **Block annotation** — Block List `default.cshtml` wraps blocks with `data-umb-block-key`; Block Grid `items.cshtml` already has `data-element-key`
- **Server-injected guest script** with CSP nonce — hover highlights (blue for properties, dark for blocks), click-to-select, postMessage to backoffice
- Side panel slides open on click showing editable inputs for document text properties (read-only for blocks)
- **Optimistic live preview** — typing in the panel instantly updates the text on the rendered page via postMessage (no save required)
- SignalR PreviewHub connection for live refresh after save (server-rendered source of truth)
- **Selection preserved** across save/refresh — panel stays open, value refreshes, guest script re-highlights

**What was NOT built:**
- No server-side partial re-render for complex properties (RichText HTML, blocks)
- No inline editing directly on the page (editing via side panel only)
- No block add/remove/reorder
- No real `<umb-property>` editors in the panel (basic HTML inputs only)
- No cross-origin live updates (postMessage parent→iframe requires same-origin; falls back to save-and-refresh)

**Developer impact**: None. Works with existing templates. Zero template changes.

**Key technical discoveries (see Appendix C for details):**
- `.NET 10 has `Write(string?)` as a separate virtual method` on `RazorPageBase` — must override both `Write(object?)` and `Write(string?)`
- Razor attribute rendering uses `BeginWriteAttribute`/`EndWriteAttribute` — a separate code path from `Write()` that must clear the tracker
- ModelsBuilder calls `PublishedContentExtensions.Value<T>()` in Umbraco.Core, not `FriendlyPublishedContentExtensions.Value<T>()` in Web.Common
- Preview URL from the API is the backoffice preview app route, not the content URL — must build `new URL(contentKey, serverUrl)` directly
- In dev mode (Vite on port 5173), iframe is cross-origin to the Umbraco server (port 44339) — guest script must be server-injected, not programmatic
- CSP blocks inline scripts — must use `ICspNonceService` nonce on the injected script tag
- PostMessage parent→iframe only works same-origin — live preview updates require built backoffice or production deployment
- Iframe `sandbox` attribute removed — not needed for trusted same-origin preview, and interferes with cross-origin postMessage

### Phase 2: Real Property Editors + Block Editing (COMPLETE)

**Goal**: Native Umbraco property editors via routed sidebar modals. Block property editing. Block add.

**What was built:**
- **Routed sidebar modal** via `UmbModalRouteRegistrationController` — integrates with browser back button, deep linking, and page reload
- Custom modal token (`UMB_VISUAL_EDITOR_PROPERTY_MODAL`) with `<umb-property-dataset>` + `<umb-property>` inside `<uui-box>` + `<umb-body-layout>`
- Property editor UI alias + config resolved via `UmbDataTypeDetailRepository.requestByUnique()` — full editor configuration (character limits, input types, placeholders)
- Block content type property structures resolved via `UmbDocumentTypeDetailRepository.requestByUniques()` — real property editors for block editing
- Block helper module (`visual-editor-block-helper.ts`) for immutable block value JSON manipulation (find, update, add blocks)
- "Update" button writes values back to workspace context + optimistic iframe text update
- "+" buttons injected between blocks by the guest script for block addition
- Modal URL is deep-linkable: `/modal/umb-modal-visualeditorproperty/invariant/property/heroHeader/`

**Key technical discoveries:**
- `UmbModalRouteRegistrationController` works in workspace view elements (not just workspace contexts)
- `.open()` uses the patched `history.pushState` which dispatches `changestate` events for the router
- `onSetup` must wait for async dependencies (workspace context + property structures) on page reload — the router calls it immediately when the URL matches, before `consumeContext` resolves
- The modal is called twice on initial load (router processes URL, then component re-renders) — `onSetup` must be idempotent
- Route params (`:editType/:editKey`) allow the same modal registration to handle both property and block editing

**What's remaining:**
- Block editing should ideally open the existing `UMB_BLOCK_WORKSPACE_MODAL` (full workspace with tabs, settings, validation) rather than our custom property list modal
- Block catalogue modal (`UMB_BLOCK_CATALOGUE_MODAL`) should be used for the "add block" type picker instead of auto-selecting the first type
- Block value write-back via block manager context (currently uses raw JSON manipulation)

### Phase 3: Block Manipulation + Partial Re-render

**Goal**: Blocks can be added, removed, and reordered visually. Changed regions re-render without full page reload.

**What to build:**
- Block add/remove/reorder via visual UI (overlay "+" buttons, drag handles)
- Partial re-render API endpoint (extends BlockPreview pattern)
- `PropertyOverridePublishedContent` decorator for rendering with unsaved values
- Guest script DOM patching for partial updates

### Phase 4: Inline Editing + Advanced Features

**Goal**: Text properties editable directly on the page. Polish and performance.

**What to build:**
- `contenteditable` for TextBox and TextArea properties
- Debounced partial re-render on blur
- Keyboard shortcuts (Escape to deselect, Ctrl+S to save)
- Responsive preview (device switching while editing)
- Undo/redo at visual editor level

---

## 7. Technical Challenges & Mitigations

### 7.1 Property Values Used in HTML Attributes (SOLVED in PoC)

**Challenge**: Properties like `colorTheme` are used in CSS class attributes (`<body class="theme-@Model.ColorTheme">`). Wrapping these with `<span>` produces malformed HTML.

**Solution (implemented):**
- Override `BeginWriteAttribute()`/`EndWriteAttribute()` on `UmbracoViewPage` to track attribute context
- `Write()` checks `_inAttribute` flag — skips annotation when inside an attribute
- `BeginWriteAttribute()` and `EndWriteAttribute()` both clear the tracker to prevent stale entries leaking
- Additionally, editor-alias filtering means only text editors are annotated — dropdowns, pickers, booleans (commonly used in attributes) are never tracked

### 7.2 .NET 10 Write(string?) Overload (SOLVED in PoC)

**Challenge**: In .NET 10, `RazorPageBase` has a separate `virtual void Write(string?)` method. Razor resolves `Write(Model.StringProperty)` to `Write(string?)`, bypassing our `Write(object?)` override entirely. String property annotations were silently skipped.

**Solution (implemented):** Override both `Write(object?)` and `Write(string?)`, both delegating to a shared `TryWriteVisualEditorAnnotation()` method.

### 7.3 ModelsBuilder Value() Call Chain (SOLVED in PoC)

**Challenge**: ModelsBuilder generates properties like `public string Title => this.Value<string>(_publishedValueFallback, "title")`. This calls `PublishedContentExtensions.Value<T>()` in **Umbraco.Core**, NOT `FriendlyPublishedContentExtensions.Value<T>()` in Umbraco.Web.Common. Tracking in the wrong method meant ModelsBuilder properties were never tracked.

**Solution (implemented):** Moved `VisualEditorPropertyTracker` to `Umbraco.Core.Models.PublishedContent` and added tracking calls to `PublishedContentExtensions.Value()` and `Value<T>()` in Core.

### 7.4 Cross-Origin Iframe in Dev Mode (SOLVED in PoC)

**Challenge**: In dev mode, the backoffice runs on Vite (port 5173) while the Umbraco server is on port 44339. The iframe is cross-origin, so `contentDocument` access fails and programmatic script injection doesn't work. Additionally, `window.document.baseURI` resolves to the Vite server, not the Umbraco server.

**Solution (implemented):**
- Build iframe URL from `serverUrl` (from `UMB_SERVER_CONTEXT`), not `baseURI`: `new URL(contentKey, serverUrl)`
- The preview URL from the API (`preview?id=...`) is the backoffice preview app route — NOT the content URL. Must use `new URL(documentUnique, serverUrl)` to get the actual rendered page
- Guest script injected server-side by `UmbracoViewPage.WriteUmbracoContent()` into `<body>`, not programmatically from the backoffice

### 7.5 Content Security Policy (SOLVED in PoC)

**Challenge**: The injected guest script is inline, blocked by CSP `script-src` directive.

**Solution (implemented):** Use `ICspNonceService.GetNonce()` to add the request's CSP nonce to the script tag: `<script data-umb-visual-editor nonce="...">`.

### 7.6 Stale Tracker Entries from Code Blocks

**Challenge**: `@{ var x = Model.Value("prop"); }` calls `Value()` (recording access) but doesn't call `Write()`. The stale entry could leak to the next `Write()` call, annotating unrelated content.

**Solution (implemented):** `WriteLiteral()` override clears the tracker. Razor always emits `WriteLiteral()` for static HTML between expressions. So `@{ code block }` → stale access → `WriteLiteral("<div>...")` → clears it → next `Write()` starts clean.

### 7.7 Optimistic Text Updates (SOLVED in PoC)

**Challenge**: Editing a property in the panel should immediately update the rendered page without waiting for a save + server re-render cycle.

**Solution (implemented):**
- On text input in the panel, send `umb:ve:update-property-text` postMessage to the iframe
- Guest script receives the message and sets `textContent` on the matching `<span data-umb-property="alias">` element
- No server round-trip — the update is instant
- On save, the full page re-renders server-side via SignalR (source of truth), replacing the optimistic update with the Razor-rendered version

### 7.8 PostMessage Direction: Parent → Iframe Requires Same-Origin (DISCOVERED in PoC)

**Challenge**: In dev mode, the backoffice runs on Vite (port 5173) while the Umbraco server is on port 44339. PostMessage from the iframe → parent works cross-origin (this is how property/block clicks reach the backoffice). But postMessage from parent → iframe **does not work** cross-origin — the iframe's `window.addEventListener('message', ...)` never fires.

**Cause**: The iframe `sandbox` attribute and/or browser security policy blocks incoming postMessage to a cross-origin iframe embedded in a sandboxed context.

**Solution**:
- **Production**: Same-origin — no issue. Backoffice and frontend are on the same domain.
- **Dev mode**: Use built backoffice (`npm run build`) instead of Vite dev server, so everything runs on port 44339.
- The iframe `sandbox` attribute was removed (not needed for a trusted preview page from the same Umbraco server).
- **Future option**: A Vite proxy configuration could route preview requests through port 5173 to make dev mode same-origin.

**Impact**: Live preview updates (typing in panel → page updates) only work same-origin. Cross-origin falls back to save-and-refresh (SignalR reload).

### 7.9 Selection Persistence Across Iframe Refresh (SOLVED in PoC)

**Challenge**: After save → SignalR → iframe reload, the user's selected property/block loses its highlight and the panel shows stale values.

**Solution (implemented):**
- `#restoreSelection()` called after iframe load — re-sends `umb:ve:select-region` message to guest script with the previously selected region ID
- Guest script re-highlights the matching element and scrolls to it
- Panel refreshes its value from the workspace context (which has the saved data)

### 7.10 Performance of Partial Re-renders (NOT YET ADDRESSED)

**Challenge**: Complex properties (RichText, blocks) can't use simple `textContent` swap — they need server-side Razor rendering.

**Planned mitigation:**
- Partial re-render API endpoint (extends BlockPreview pattern)
- Debounce edits (500ms)
- Concurrency queue (max 3 requests)
- Full page reload as fallback for structural changes

### 7.11 Complex Nested Blocks (NOT YET ADDRESSED)

**Challenge**: Block Grid with nested areas, blocks within blocks.

**Planned mitigation:**
- Block Grid `items.cshtml` already has `data-element-key` on each item
- Block Grid `area.cshtml` already has `data-area-alias` on each area
- Nested block editing opens the block's property editors in the panel
- Drop zones within areas for block insertion (Phase 3)

---

## 8. Files Created or Modified (PoC)

### 8.1 New Files (Created in PoC)

**Backend (.NET):**

| File | Project | Purpose |
|------|---------|---------|
| `Models/PublishedContent/VisualEditorPropertyTracker.cs` | `Umbraco.Core` | AsyncLocal tracker — records property accesses during Razor rendering, enabled per-request in preview mode |
| `Views/VisualEditorGuestScript.cs` | `Umbraco.Web.Common` | Guest script as C# constant, injected server-side into `<body>` with CSP nonce. Handles property + block hover/click, "+" block insertion buttons, optimistic text updates, selection restore |

**Frontend (TypeScript):**

| File | Package | Purpose |
|------|---------|---------|
| `workspace/views/visual-editor/document-workspace-view-visual-editor.element.ts` | `documents` | Main workspace view — iframe, routed modal registration, SignalR, postMessage handling |
| `workspace/views/visual-editor/visual-editor-property-modal.token.ts` | `documents` | Modal token — defines data/value interfaces for the property/block editing sidebar modal |
| `workspace/views/visual-editor/visual-editor-property-modal.element.ts` | `documents` | Modal element — `<umb-property-dataset>` + `<umb-property>` with Update/Close actions |
| `workspace/views/visual-editor/visual-editor-block-helper.ts` | `documents` | Block value JSON helpers — find block in values, update block properties, add new block (immutable) |

### 8.2 Modified Files (Changed in PoC)

**Backend (.NET):**

| File | Change |
|------|--------|
| `src/Umbraco.Core/Extensions/PublishedContentExtensions.cs` | Added `TrackVisualEditorAccess()` calls in `Value()` and `Value<T>()` with editor-alias filtering |
| `src/Umbraco.Web.Common/Views/UmbracoViewPage.cs` | Added `Write(string?)` override, `TryWriteVisualEditorAnnotation()`, `BeginWriteAttribute`/`EndWriteAttribute` overrides, `WriteLiteral` override, guest script injection in `WriteUmbracoContent()` |
| `src/Umbraco.Web.Common/Middleware/PreviewAuthenticationMiddleware.cs` | Added `VisualEditorPropertyTracker.Enable()` call when preview cookie detected |

**Frontend (TypeScript):**

| File | Change |
|------|--------|
| `src/packages/documents/documents/workspace/manifests.ts` | Registered `Umb.WorkspaceView.Document.VisualEditor` workspace view with conditions |

**Partial Views:**

| File | Change |
|------|--------|
| `src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml` | Wrapped each block in `<div data-umb-block-key data-umb-content-type>` |

### 8.3 Files for Future Phases

| File | Project | Purpose | Phase |
|------|---------|---------|-------|
| `Controllers/VisualEditor/RenderRegionController.cs` | `Umbraco.Cms.Api.Management` | Partial re-render API | 3 |
| `Services/PropertyOverridePublishedContent.cs` | `Umbraco.Infrastructure` | IPublishedContent decorator with value overrides | 3 |
| `TagHelpers/VisualEditorRegionTagHelper.cs` | `Umbraco.Web.Common` | Optional `<umb-visual-region>` for non-text properties | 3+ |

---

## 9. Open Questions

### Answered by PoC

1. **~~Separate cookie vs query parameter for visual edit mode?~~**
   - **Answer**: Neither. Reuse the existing `UMB_PREVIEW` cookie. The visual editor calls `getPreviewUrl()` to set it, then the preview middleware enables the property tracker. No new cookie needed.

2. **~~Should visual edit mode imply preview mode, or be independent?~~**
   - **Answer**: Same thing. Visual edit mode IS preview mode. The tracker is enabled for all preview requests. The annotations are lightweight enough that they don't affect normal preview.

3. **~~How to handle templates that use `Layout` (master pages)?~~**
   - **Answer**: Works automatically. Razor executes the child page first (buffered), then the layout. The tracker's AsyncLocal flows correctly across both. Properties from both the page and the layout are annotated.

4. **~~Developer-authored Razor views — how to annotate without template changes?~~**
   - **Answer**: Automatic annotation via the Razor pipeline. `Value<T>()` records the access, `Write(string?)` wraps the output. Zero template changes needed for text properties. The editor-alias convention filters to safe property types only.

11. **Standalone window vs embedded workspace view?**
    - **Answer**: Standalone window. The visual editor will run as a separate app entry point (like `umb-preview`) in its own browser tab/window. Editing modals appear directly over the preview — no tab-switching. See [Architecture Evolution: Standalone Window](#10-architecture-evolution-standalone-window) for full design.

### Still Open

5. **Property type setting vs editor-alias convention for controlling annotation?**
   - Current: hard-coded list of editor aliases (TextBox, TextArea, RichText, MarkdownEditor)
   - Alternative: property type appearance setting "Editable in Visual Editor" — more flexible, allows per-property control
   - Requires: backend model change, migration, API DTO, frontend toggle in property type settings
   - Recommendation: Start with convention, add the setting in Phase 2 if needed

6. **Should the visual editor be the default view for documents with templates?**
   - Current: opt-in tab (weight 150, between Content at 200 and Info at 100)
   - Consider making it the first tab in a future version

7. **How to handle responsive design in the visual editor?**
   - The iframe can be resized (reuse preview's device selector pattern)
   - Guest script would need to re-report region positions on resize

8. **BlockPreview compatibility?**
   - BlockPreview's rendering API could be reused for partial re-render in Phase 3
   - The annotation system is complementary — BlockPreview renders blocks in the backoffice editor, visual editor annotates blocks on the rendered page

9. **How to handle non-text properties that ARE rendered as visible content?**
   - Example: a media picker property whose image is displayed — clicking the image should open the media picker
   - The automatic annotation doesn't cover this (media pickers are filtered out)
   - Options: opt-in TagHelper, or extend the editor-alias list to include MediaPicker3 with special handling

10. **Performance impact of tracking on every `Value()` call?**
    - The `IsEnabled` check is a single `AsyncLocal<bool>` read — near-zero cost when disabled (non-preview requests)
    - When enabled, the `GetProperty()` + `DataType.EditorAlias` check adds a small overhead per property access
    - Needs benchmarking with large pages (many properties, many partials)

---

## 10. Architecture Evolution: Standalone Window

### 10.1 Motivation

The PoC (Phases 1-2) embedded the visual editor as a workspace view tab, which gave free access to `UMB_DOCUMENT_WORKSPACE_CONTEXT` but constrained the editing experience to a panel within the backoffice shell. Problems:

- **Limited canvas**: The iframe shares space with the workspace header, tabs, sidebar, and footer
- **Context switching**: Users must mentally switch between "editing mode" (visual) and "navigation mode" (backoffice) within the same window
- **Modal stacking**: Property editing modals compete with the workspace's own modal system

The preview app (`umb-preview`) already proves that a standalone backoffice window works — it bootstraps auth, loads extensions, and renders UUI components independently. The visual editor should follow the same pattern but with full editing capabilities.

### 10.2 Architecture: Standalone Visual Editor App

```
┌─────────────────────────────────────────────────────────────────────┐
│  Visual Editor Window (standalone tab/window)                        │
│  Entry point: /umbraco/visual-editor?id={key}&culture=&segment=      │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  Bootstraps: UMB_AUTH_CONTEXT + Extensions + UUI               │ │
│  │  Provides:   UmbVisualEditorContext (document state via API)    │ │
│  │  Provides:   UmbModalManagerContext (for editing modals)        │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  ┌───────────────────────────────────┐  ┌────────────────────────┐  │
│  │  Iframe (full rendered page)      │  │  Sidebar Modal         │  │
│  │  ┌─────────────────────────────┐  │  │  (slides over iframe)  │  │
│  │  │ Annotated regions           │  │  │                        │  │
│  │  │ data-umb-property="..."     │  │  │  ┌──────────────────┐  │  │
│  │  │ data-umb-block-key="..."    │  │  │  │ <umb-property>   │  │  │
│  │  │                             │  │  │  │ Native editors    │  │  │
│  │  │ Guest script handles:       │  │  │  │ Full validation   │  │  │
│  │  │ - hover/click → postMessage │  │  │  │ Full config       │  │  │
│  │  │ - "+" block buttons         │  │  │  └──────────────────┘  │  │
│  │  │ - optimistic text updates   │  │  │                        │  │
│  │  └─────────────────────────────┘  │  │  [Update] [Close]      │  │
│  └───────────────────────────────────┘  └────────────────────────┘  │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  Toolbar: [Save] [Publish] [Culture ▾] [Device ▾] [← Back]    │ │
│  └─────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  Backoffice Window (existing, unchanged)                             │
│  Receives BroadcastChannel messages: "document saved externally"     │
│  Workspace refreshes its state from API                              │
└─────────────────────────────────────────────────────────────────────┘
```

### 10.3 File Structure

```
src/Umbraco.Web.UI.Client/src/
├── apps/
│   ├── preview/                          # Existing preview app (unchanged)
│   │   └── preview.element.ts
│   └── visual-editor/                    # NEW: Standalone visual editor app
│       ├── visual-editor.element.ts      # Entry point element (like preview.element.ts)
│       └── visual-editor.context.ts      # Document state management via API
│
├── packages/
│   ├── visual-editor/                    # NEW: Visual editor package
│   │   ├── context/
│   │   │   └── visual-editor.context-token.ts
│   │   ├── components/
│   │   │   ├── visual-editor-toolbar.element.ts    # Save/publish/culture/device bar
│   │   │   ├── visual-editor-sidebar.element.ts    # Property editing sidebar
│   │   │   └── visual-editor-block-picker.element.ts  # Block type selection
│   │   ├── modals/
│   │   │   ├── visual-editor-property-modal.element.ts  # Reuse from PoC
│   │   │   └── visual-editor-property-modal.token.ts
│   │   └── umbraco-package.ts            # Extension manifest registration
│   │
│   └── documents/documents/workspace/
│       └── views/visual-editor/          # Existing PoC files (KEPT as-is)
│           ├── document-workspace-view-visual-editor.element.ts
│           ├── visual-editor-block-helper.ts
│           ├── visual-editor-property-modal.element.ts
│           └── visual-editor-property-modal.token.ts
```

### 10.4 Key Components

#### 10.4.1 Visual Editor App Entry Point

Follows the same pattern as `preview.element.ts`:

```typescript
// apps/visual-editor/visual-editor.element.ts
@customElement('umb-visual-editor-app')
export class UmbVisualEditorAppElement extends UmbLitElement {
    #context = new UmbVisualEditorContext(this);

    constructor() {
        super();
        // Bootstrap extensions (same as preview)
        new UmbBackofficeEntryPointExtensionInitializer(this, umbExtensionsRegistry);
    }

    override async firstUpdated() {
        // Wait for auth (same as preview)
        const authContext = await this.getContext(UMB_AUTH_CONTEXT, { preventTimeout: true });
        await this.observe(authContext.isAuthorized).asPromise();
        await new UmbServerExtensionRegistrator(this, umbExtensionsRegistry)
            .registerPrivateExtensions();

        // Load visual editor extensions
        const { extensions } = await import('../../packages/visual-editor/umbraco-package.js');
        umbExtensionsRegistry.registerMany(extensions);
    }

    #onIFrameLoad(event: Event & { target: HTMLIFrameElement }) {
        this.#context.iframeLoaded(event.target);
    }

    override render() {
        return html`
            <div id="visual-editor-app">
                <div id="canvas">
                    <iframe
                        src=${ifDefined(this.#context.previewUrl)}
                        title="Visual Editor"
                        @load=${this.#onIFrameLoad}>
                    </iframe>
                </div>

                <!-- Sidebar renders over the canvas when a region is selected -->
                <umb-visual-editor-sidebar></umb-visual-editor-sidebar>

                <!-- Bottom toolbar -->
                <umb-visual-editor-toolbar></umb-visual-editor-toolbar>
            </div>
        `;
    }
}
```

#### 10.4.2 Visual Editor Context (API-Driven)

Replaces workspace context dependency with direct API calls:

```typescript
// apps/visual-editor/visual-editor.context.ts
export class UmbVisualEditorContext extends UmbControllerBase {
    // State (parsed from URL query params, same as preview.context.ts)
    #unique = new UmbStringState('');
    #culture = new UmbStringState('');
    #segment = new UmbStringState('');

    // Document data (fetched via API)
    #documentData = new UmbObjectState<DocumentResponseModel | undefined>(undefined);
    #contentTypeData = new UmbObjectState<DocumentTypeResponseModel | undefined>(undefined);
    #propertyValues = new UmbObjectState<Record<string, unknown>>({});

    // Iframe state
    #previewUrl = new UmbStringState('');
    #iframeReady = new UmbBooleanState(false);

    // SignalR connection (same as preview)
    #connection?: HubConnection;

    // BroadcastChannel for notifying the backoffice tab
    #channel = new BroadcastChannel('umb:visual-editor');

    readonly unique = this.#unique.asObservable();
    readonly culture = this.#culture.asObservable();
    readonly previewUrl = this.#previewUrl.asObservable();
    readonly iframeReady = this.#iframeReady.asObservable();

    constructor(host: UmbControllerHost) {
        super(host);

        // Parse URL params (same pattern as preview.context.ts)
        const params = new URLSearchParams(window.location.search);
        this.#unique.setValue(params.get('id') ?? '');
        this.#culture.setValue(params.get('culture') ?? '');
        this.#segment.setValue(params.get('segment') ?? '');

        this.provideContext(UMB_VISUAL_EDITOR_CONTEXT, this);
    }

    async initialize() {
        // 1. Enter preview mode (sets cookie)
        await DocumentService.putDocumentByIdPreview({ id: this.#unique.getValue() });

        // 2. Fetch document data via API
        const doc = await DocumentService.getDocumentById({ id: this.#unique.getValue() });
        this.#documentData.setValue(doc);

        // 3. Fetch content type for property structure
        const contentType = await DocumentTypeService.getDocumentTypeById({
            id: doc.documentType.id
        });
        this.#contentTypeData.setValue(contentType);

        // 4. Build property values map from document values
        const values: Record<string, unknown> = {};
        for (const variant of doc.values) {
            values[variant.alias] = variant.value;
        }
        this.#propertyValues.setValue(values);

        // 5. Build preview URL and connect SignalR
        this.#buildPreviewUrl();
        this.#connectSignalR();
    }

    // --- Property access ---

    getPropertyValue(alias: string): unknown {
        return this.#propertyValues.getValue()[alias];
    }

    setPropertyValue(alias: string, value: unknown) {
        const current = this.#propertyValues.getValue();
        this.#propertyValues.setValue({ ...current, [alias]: value });
    }

    getPropertyStructure(alias: string) {
        const contentType = this.#contentTypeData.getValue();
        // Find property in content type's property collection
        return contentType?.properties?.find(p => p.alias === alias);
    }

    // --- Save/publish via API ---

    async save() {
        const doc = this.#documentData.getValue();
        if (!doc) return;

        await DocumentService.putDocumentById({
            id: this.#unique.getValue(),
            requestBody: {
                ...doc,
                values: this.#buildValuesPayload(),
            },
        });

        // Notify backoffice tab to refresh workspace
        this.#channel.postMessage({
            type: 'umb:visual-editor:document-saved',
            documentId: this.#unique.getValue(),
        });
    }

    async saveAndPublish(cultures: string[]) {
        await this.save();
        await DocumentService.putDocumentByIdPublish({
            id: this.#unique.getValue(),
            requestBody: { publishSchedules: cultures.map(c => ({ culture: c })) },
        });

        this.#channel.postMessage({
            type: 'umb:visual-editor:document-published',
            documentId: this.#unique.getValue(),
        });
    }

    // --- SignalR (same pattern as preview.context.ts) ---

    async #connectSignalR() {
        const serverContext = await this.getContext(UMB_SERVER_CONTEXT);
        const serverUrl = serverContext?.getServerUrl() ?? '';

        this.#connection = new HubConnectionBuilder()
            .withUrl(`${serverUrl}/umbraco/PreviewHub`)
            .build();

        this.#connection.on('refreshed', (payload: string) => {
            if (payload === this.#unique.getValue()) {
                this.#refreshIframe();
            }
        });

        await this.#connection.start();
    }
}
```

#### 10.4.3 Toolbar

```typescript
// packages/visual-editor/components/visual-editor-toolbar.element.ts
@customElement('umb-visual-editor-toolbar')
export class UmbVisualEditorToolbarElement extends UmbLitElement {
    #context?: UmbVisualEditorContext;

    constructor() {
        super();
        this.consumeContext(UMB_VISUAL_EDITOR_CONTEXT, (ctx) => {
            this.#context = ctx;
        });
    }

    override render() {
        return html`
            <div id="toolbar">
                <uui-button @click=${this.#goBack} look="secondary" compact>
                    <uui-icon name="icon-arrow-left"></uui-icon> Back to editor
                </uui-button>

                <div id="toolbar-center">
                    <!-- Culture picker (same as preview apps) -->
                    <umb-extension-slot type="previewApp"></umb-extension-slot>
                </div>

                <div id="toolbar-right">
                    <uui-button @click=${this.#save} look="secondary" label="Save">
                        Save
                    </uui-button>
                    <uui-button @click=${this.#saveAndPublish} look="primary" label="Save and Publish">
                        Save and Publish
                    </uui-button>
                </div>
            </div>
        `;
    }

    #goBack() {
        // Navigate back to backoffice document editor
        window.close(); // Or window.location.href = backoffice URL
    }

    async #save() {
        await this.#context?.save();
    }

    async #saveAndPublish() {
        await this.#context?.saveAndPublish([this.#context.getCulture()]);
    }
}
```

#### 10.4.4 Sidebar (Property Editing Modal Over Canvas)

```typescript
// packages/visual-editor/components/visual-editor-sidebar.element.ts
@customElement('umb-visual-editor-sidebar')
export class UmbVisualEditorSidebarElement extends UmbLitElement {
    #context?: UmbVisualEditorContext;

    @state() private _selectedAlias?: string;
    @state() private _selectedBlockKey?: string;

    constructor() {
        super();
        this.consumeContext(UMB_VISUAL_EDITOR_CONTEXT, (ctx) => {
            this.#context = ctx;
            // Listen for postMessage selections forwarded by context
            this.observe(ctx.selectedRegion, (region) => {
                this._selectedAlias = region?.propertyAlias;
                this._selectedBlockKey = region?.blockKey;
            });
        });
    }

    override render() {
        if (!this._selectedAlias && !this._selectedBlockKey) return nothing;

        const properties = this._selectedBlockKey
            ? this.#context?.getBlockPropertyStructures(this._selectedBlockKey) ?? []
            : [this.#context?.getPropertyStructure(this._selectedAlias!)].filter(Boolean);

        return html`
            <div id="sidebar" class="open">
                <umb-body-layout headline=${this._selectedAlias ?? 'Block'}>
                    <uui-box>
                        ${properties.map(prop => html`
                            <umb-property-dataset
                                .value=${this.#getValues(prop)}
                                @change=${(e: Event) => this.#onValueChange(prop.alias, e)}>
                                <umb-property
                                    alias=${prop.alias}
                                    label=${prop.name}
                                    .config=${prop.config}
                                    property-editor-ui-alias=${prop.propertyEditorUiAlias}>
                                </umb-property>
                            </umb-property-dataset>
                        `)}
                    </uui-box>
                    <div slot="actions">
                        <uui-button @click=${this.#close} label="Close">Close</uui-button>
                        <uui-button @click=${this.#update} look="primary" label="Update">Update</uui-button>
                    </div>
                </umb-body-layout>
            </div>
        `;
    }
}
```

### 10.5 Opening the Visual Editor

The visual editor window is opened from the backoffice document workspace:

```typescript
// Workspace action (registered as a manifest)
// Opens a new window/tab with the visual editor app
async #openVisualEditor() {
    const unique = this.#workspaceContext.getUnique();
    const culture = this.#workspaceContext.getCulture();
    const segment = this.#workspaceContext.getSegment();

    // Ensure document is saved first
    if (this.#workspaceContext.getIsDirty()) {
        await this.#workspaceContext.save();
    }

    const url = `/umbraco/visual-editor?id=${unique}&culture=${culture}&segment=${segment}`;
    window.open(url, `visual-editor-${unique}`);
}
```

### 10.6 Cross-Tab Communication

```
BroadcastChannel: 'umb:visual-editor'

Visual Editor → Backoffice:
  { type: 'umb:visual-editor:document-saved', documentId }
  { type: 'umb:visual-editor:document-published', documentId }

Backoffice workspace listens and refreshes its state from API.
```

Note: `BroadcastChannel` does not deliver messages to the sender's own tab — only other tabs receive them. This is exactly what we want: the visual editor saves via API and notifies the backoffice tab to refresh, without the visual editor receiving its own notification.

### 10.7 Property Editor Compatibility Risk

**Risk**: Some property editors may internally `consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT)`. These would fail in the standalone window because that context doesn't exist.

**Mitigation strategies** (in order of preference):

1. **Audit property editors**: Check which editors consume workspace context vs only using `UMB_PROPERTY_DATASET_CONTEXT` (which the sidebar provides). Most standard editors only need the dataset context.

2. **Provide a shim context**: Create a lightweight `UmbVisualEditorWorkspaceShim` that implements the subset of `UMB_DOCUMENT_WORKSPACE_CONTEXT` that property editors actually use (typically `getUnique()`, `getEntityType()`, and variant state).

3. **Graceful degradation**: If an editor fails to resolve its context, catch the error and show a fallback message: "This property type cannot be edited in the visual editor. Open in backoffice →".

### 10.8 Migration Path

The existing embedded workspace view (PoC) and the standalone window can coexist:

1. **Keep the workspace view tab** — it remains useful for quick edits without leaving the backoffice
2. **Add a workspace action button** — "Open Visual Editor" opens the standalone window
3. **Share code** — Guest script, block helpers, annotation pipeline, and property modal are reused by both approaches
4. **User preference** — Future: user setting for "default visual editor mode" (embedded vs standalone)

### 10.9 Comparison: Embedded vs Standalone

| Aspect | Embedded (Current PoC) | Standalone (New) |
|--------|----------------------|-----------------|
| Canvas size | Constrained by workspace shell | Full browser window |
| Context access | Direct (DOM tree) | Via API calls |
| Property editors | Always work (workspace context present) | Most work; some may need shim |
| Save/publish | Via workspace actions (shared pipeline) | Via API directly |
| Backoffice sync | Automatic (shared context) | BroadcastChannel notifications |
| Modal rendering | Competes with workspace modals | Full control, renders over canvas |
| User flow | Tab within editor | Separate window, "Open Visual Editor" button |
| Offline edits | Tracks dirty state in workspace | Must save to persist |
| Complexity | Lower (leverages existing workspace) | Higher (own state management, API calls) |

### 10.10 Server-Side Changes

**Minimal**. The standalone window needs:

1. **A route** for `/umbraco/visual-editor` that serves the app HTML (same pattern as `/umbraco/preview` — an HTML page that loads the `umb-visual-editor-app` custom element)
2. **No new API endpoints** — uses existing `DocumentService`, `DocumentTypeService`, and preview APIs
3. **No changes to annotation pipeline** — the iframe loads the same preview-mode page with the same guest script

The existing `PreviewAuthenticationMiddleware`, `VisualEditorPropertyTracker`, and guest script injection all work unchanged.

## Appendix A: Glossary

| Term | Definition |
|------|-----------|
| **Region** | An annotated area of the rendered page that maps to a property or block |
| **Guest Script** | JavaScript injected into the rendered page iframe for communication and interaction |
| **Overlay** | Transparent layer in the backoffice shell positioned over the iframe for selection/hover UI |
| **Panel** | Side panel showing property editors for the selected region |
| **Partial Re-render** | Server-side rendering of a single region with updated property values |
| **Annotation** | `data-umb-*` HTML attributes that identify editable regions |
| **Inline Editing** | Direct text editing on the rendered page via `contenteditable` |

## Appendix B: Annotation Attribute Reference

**Implemented in PoC:**

| Attribute | Emitted By | Value | Purpose |
|-----------|-----------|-------|---------|
| `data-umb-property` | `UmbracoViewPage.Write()` | Property alias | Auto-annotated text property output |
| `data-umb-content-key` | `UmbracoViewPage.Write()` | GUID | Content item key (on property spans) |
| `data-umb-block-key` | `blocklist/default.cshtml` | GUID | Block content key (Block List) |
| `data-umb-content-type` | `blocklist/default.cshtml` | Alias | Block's content type alias |
| `data-element-key` | `blockgrid/items.cshtml` (existing) | GUID | Block content key (Block Grid) |
| `data-content-element-type-alias` | `blockgrid/items.cshtml` (existing) | Alias | Block's content type alias (Block Grid) |

**Planned for future phases:**

| Attribute | Purpose |
|-----------|---------|
| `data-umb-block-area` | Block grid area identifier (for drop zones) |
| `data-umb-inline-edit` | Enables inline contenteditable editing |

## Appendix C: PoC Technical Findings

### The Automatic Property Annotation Mechanism

The PoC proved that text properties can be automatically annotated in the Razor output without any template changes. The mechanism:

```
Request with UMB_PREVIEW cookie
  → PreviewAuthenticationMiddleware enables VisualEditorPropertyTracker (AsyncLocal)
  → Razor renders the page
  → For each @Model.Property or @Model.Value("alias"):
      1. ModelsBuilder calls PublishedContentExtensions.Value<T>(content, fallback, alias)
      2. Value<T>() looks up content.GetProperty(alias).PropertyType.DataType.EditorAlias
      3. If editor is TextBox/TextArea/RichText/MarkdownEditor → records (alias, contentKey) in AsyncLocal
      4. Razor calls UmbracoViewPage.Write(string?) with the property value
      5. Write() consumes the recorded access → wraps output with <span data-umb-property="...">
      6. WriteLiteral() between expressions clears any unconsumed stale access
      7. BeginWriteAttribute()/EndWriteAttribute() prevent annotation inside HTML attributes
```

### Key .NET 10 Discovery: Write(string?) Overload

`RazorPageBase` in .NET 10 has a **separate virtual method** `Write(string?)` alongside `Write(object?)`. The C# compiler resolves `Write(Model.StringProperty)` to `Write(string?)` because `string` is a more specific type than `object`. This means overriding only `Write(object?)` misses all string property renders.

**Fix**: Override both, delegating to a shared annotation method.

### Preview URL vs Content URL

The `UmbPreviewRepository.getPreviewUrl()` API returns a URL like `preview?id={guid}&culture=&segment=`. This is the **backoffice preview app** route (handled by `preview.element.ts`), NOT the rendered page URL.

For the visual editor iframe, the actual content URL must be built as `new URL(contentKeyGuid, serverUrl)` — the same pattern used by `preview.context.ts:162`. The API call is still needed to set the preview cookie, but the returned URL is not used for the iframe src.

### Server-Side Script Injection

The guest script cannot be injected programmatically from the backoffice into the iframe when cross-origin (dev mode: Vite on port 5173, server on port 44339). The solution is server-side injection via `UmbracoViewPage.WriteUmbracoContent()`, which already injects the preview badge into `<body>`. The script tag requires the CSP nonce from `ICspNonceService`.

### PostMessage Directionality

PostMessage between the backoffice (parent) and the preview iframe works **asymmetrically** in cross-origin dev mode:

| Direction | Cross-origin (dev) | Same-origin (prod) |
|-----------|-------------------|-------------------|
| Iframe → Parent (clicks, region-map) | Works | Works |
| Parent → Iframe (live updates, selection restore) | **Does NOT work** | Works |

This means live preview updates (typing in panel → page updates instantly) only work in production or when using the built backoffice on the same origin. In cross-origin dev mode, the user must save to see changes (SignalR triggers full iframe reload).

The `sandbox` attribute on the iframe was removed as it's not needed for trusted same-origin preview pages and was contributing to the cross-origin postMessage issues.

### Optimistic Text Updates

For text properties (TextBox, TextArea), the panel sends `umb:ve:update-property-text` via postMessage to the guest script, which does a simple `span.textContent = newValue`. This is instant — no server round-trip. The server re-render on save replaces the optimistic content with the Razor-rendered version (source of truth).

This approach works for plain text. For RichText (HTML content) and properties whose rendered output differs from their stored value (e.g., Markdown → HTML), a server-side partial re-render API will be needed (Phase 2+).

---

## 11. Discussion Notes & Feedback

> Captured 2026-03-27 — early feedback on the visual editor direction.

### General Sentiment

Full page preview is the right direction — this is the way forward for visual editing in Umbraco.

### Block Manipulation Challenges

Moving blocks **between areas** or **reordering blocks** within the preview will be tricky. The block ordering/drag-and-drop logic currently lives deep inside the Block Grid and Block List packages. To make this work in the visual editor, we need to **abstract the block manipulation logic up to the library/view level** so it can be driven from the preview iframe context, not just from the backoffice block editor UI.

This ties into the existing Phase 3 block manipulation work (§4.6) but highlights that the abstraction boundary needs to be higher than currently planned.

### Inline Block Editing

Currently there is **no support for editing individual blocks** inline within the visual editor. The PoC handles property-level inline editing for text properties, but block-level editing (opening a block's properties, changing its content/settings) is not yet wired up. This is a prerequisite for the visual editor to be useful on block-heavy pages.

### Headless Rendering Support

The visual editor must support **both template-based rendering and headless rendering**. Currently the architecture assumes server-side Razor rendering for the preview iframe. For headless sites (using the Delivery API), there is no Razor template to render.

**Action**: Investigate [Kenn's headless preview package](https://github.com/kjac) for inspiration on how headless preview rendering is handled. The visual editor should be able to render a headless frontend (e.g., Next.js, Nuxt) in the iframe alongside the editing overlay, using the same postMessage protocol.

This may require:
- A configurable preview URL source (template URL vs external frontend URL)
- A way for headless frontends to opt into the guest script injection (or load it themselves)
- Delivery API support for draft/preview content in the iframe context

### Code Deduplication

There is duplicated logic between the **Block Grid** and **Block List** packages that should be consolidated. As we build the visual editor's block manipulation layer, this is the right time to extract shared code into common utilities. The new `src/packages/block/block/utils/` directory (visible in current working changes) is a step in this direction.

### Validation in Preview

Validation in the visual editor preview will be **tricky but achievable**. Currently, validation runs in the editing overlay/modal — the standard property editor validation pipeline fires when saving from the workspace. Making validation visible directly in the preview (e.g., highlighting invalid blocks, showing inline validation messages) requires surfacing validation state in the iframe guest script.

This is durable once implemented — the workspace's existing validation pipeline remains the source of truth, and the preview simply reflects its state visually.

### Document Type–Controlled Editability

The document type should be able to **decide which property editors are editable in the visual editor**. This extends Open Question #5 (property type setting vs editor-alias convention). Rather than a hard-coded list of "inline-editable" editor aliases, the document type or property type configuration should include an "Editable in Visual Editor" toggle.

This gives content architects control over which properties appear as interactive in the preview versus read-only rendered output.

### Scroll Position Retention

Need to introduce **scroll position retention** so that when the preview iframe re-renders (after save, after property change), the user's scroll position is preserved. Currently a full iframe reload loses scroll state. Options:
- Store `scrollTop` before reload, restore after `load` event
- For partial re-renders (Phase 2+), no scroll change needed since the page doesn't reload
- The guest script could report and restore scroll position via postMessage

### Responsive UI Controls

The visual editor should reuse the **existing preview responsive controls** (device selector, viewport resizing). The preview app already supports device simulation — the visual editor should inherit this capability so editors can see and edit content at different breakpoints.

### Save & Preview → Visual Editor Fullscreen

The **Save and Preview** button could navigate directly to the visual editor experience in fullscreen mode (standalone window) rather than the current preview-only view. This creates a natural workflow:
- Edit in form view → Save and Preview → lands in visual editor (fullscreen, standalone window)
- The visual editor IS the preview, but with editing capabilities
- This aligns with the standalone window architecture (§10) and makes the visual editor the default "preview" destination
