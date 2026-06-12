# Visual Editor — Partial Re-render (Phase 3 remainder) — Design

**Status**: Implemented (spike passed 2026-06-11; see `2026-06-11-visual-editor-partial-rerender-plan.md`). Built via cache-node override + `IPublishedContentFactory` rather than a decorator — see the plan's "Deliberate deviation" note.
**Date**: 2026-06-11
**Author**: Rick Butterfield + Claude
**Scope**: The "Still to build" items of Phase 3 in `docs/plans/visual-page-builder.md` — server-side partial re-render with unsaved values, and client-side DOM patching. Block manipulation itself is already done.
**Relates to**: `docs/plans/visual-page-builder.md` §4.4 (original endpoint sketch), §2.3 (BlockPreview pattern); supersedes the isolated-region endpoint idea in §4.4 in favour of full-page render + client morph.

---

## Goal

Make partial re-render the **single, universal mechanism** for reflecting edits in the visual editor preview, retiring both the optimistic-text-only path and the save-and-full-reload path. Every edit — plain text, RTE/Markdown/media, block content/settings, and structural block add/delete/move/reorder — is reflected by re-rendering the page server-side with the workspace's unsaved values and morphing the live iframe DOM in place (no reload, scroll/selection preserved).

## Decisions locked

| Decision | Outcome |
|---|---|
| Trigger scope | **All** edit types route through re-render: block content/settings, block add/delete/move/reorder, RTE/Markdown/transformed properties, and plain text. |
| Feedback model | **Optimistic + authoritative**: instant optimistic `textContent` paint for plain text on keystroke; a debounced (~500ms) server re-render then replaces the region with true Razor output. Blocks/RTE show a subtle pending state (no meaningful optimistic paint) until the render returns. |
| Rendering approach | **A — full-page render + client DOM morph.** One endpoint renders the whole page via the existing preview path with unsaved values injected; guest morphs the live DOM. Chosen over isolated-region (B) because only a full-page render covers arbitrary-template property placement with guaranteed fidelity, and over hybrid (C) for single-path simplicity. |
| DOM patch | Bundle **morphdom** in the guest bundle; morph `<body>`, touching only changed nodes; preserves scroll. |
| Failure mode | **Keep last good DOM + quiet notice.** Leave current DOM untouched, log, transient non-blocking indicator; the workspace already holds the edit so the next successful render reconciles. Never silently swallow. |
| Save + SignalR | **Suppress self-reload, keep as multi-user net.** After a local save, a short-lived guard makes the editor ignore its own `refreshed` SignalR event (DOM already authoritative — no flicker). Refreshes not caused by this editor still reload. |

## Architecture & data flow

```
edit (property / block / structural)
  → element updates workspace value (source of truth)  [+ optimistic textContent for plain text]
  → UmbVisualEditorRenderController: debounce ~500ms, latest-wins (AbortController cancels in-flight)
  → POST /umbraco/management/api/v1/visual-editor/render
        body: { unique, culture?, segment?, values: [{ alias, value, culture?, segment? }] }
  → server:
        EnsureUmbracoContext + force preview mode + VisualEditorPropertyTracker.Enable() for the render scope
        base = DRAFT content from the published cache (preview read — same as the iframe shows)
        wrap in PropertyOverridePublishedContent(unsaved values)
        render the assigned template → HTML string (data-umb-* annotations emitted)
        → { html }
  → element posts umb:ve:render to the guest with the HTML
  → guest morphs <body> (morphdom) → re-runs initRegions() → restores selection highlight
```

The base is the **draft** content the iframe already renders (preview-mode cache read); the override layer is the workspace's even-newer unsaved edits on top.

## Server components (new)

| Unit | Project | Responsibility |
|---|---|---|
| Override-content builder (conversion) | `Umbraco.PublishedCache.HybridCache` (or a public seam exposed from it) | Produce an `IPublishedContent` representing the draft + unsaved overrides. **Approach proven by the spike** (and mirroring the in-tree `BlockElementService.BuildElementAsync`): for each overridden alias, run the editor-format value through `dataType.Editor.GetValueEditor().FromEditor(new ContentPropertyData(value, dataType.ConfigurationObject), null)` to get the source value; reuse the existing saved source values (`property.GetValue(published)`) for non-overridden aliases; assemble `PropertyData[]` → `ContentData` → `ContentCacheNode` → `IPublishedContentFactory.ToIPublishedContent(node, preview: true).CreateModel(...)`. Threads `Culture`/`Segment` onto `PropertyData` and sets `ContentData.CultureInfos` for variant content. **Not** a `GetProperty` decorator — a cache-node rebuild. (`IPublishedContentFactory` is `internal` to HybridCache, hence this unit lives there or a small public seam is added — resolved in the plan.) |
| `IVisualEditorRenderService` + impl | `Umbraco.Web.Common` | Renders a supplied `IPublishedContent` to an HTML string. Modeled on `TemplateRenderer` (`src/Umbraco.Web.Common/Templates/TemplateRenderer.cs`): build an `IPublishedRequest` via `IPublishedRouter`, `SetPublishedContent(overriddenContent)`, set culture/segment + template, swap onto `UmbracoContext.PublishedRequest`, render the template view to a `StringWriter`, restore. Forces preview mode + enables `VisualEditorPropertyTracker` for the render scope so annotations are emitted. RTE-embedded blocks render via the partial-view block engine, which this render context satisfies. |
| `RenderVisualEditorController` | `Umbraco.Cms.Api.Management` | `POST /umbraco/management/api/v1/visual-editor/render`, `[Authorize(Policy = BackOfficeAccess)]`. Ensures an `UmbracoContext`, resolves the draft content for `unique`, builds the override content from the request `values`, calls the render service, returns `{ html }`. |

**Value conversion — DE-RISKED by the spike (2026-06-11).** All three property kinds convert correctly via `IPublishedContentFactory.ToIPublishedContent`:
- **TextBox** — `FromEditor` → string source → published string. Clean.
- **Rich Text** — `FromEditor` → source JSON → `RteBlockRenderingValueConverter`; all link/url/image parsing happens at value-conversion time (no `IPublishedRequest` needed). RTE-*embedded blocks* additionally use the partial-view block engine at render time (covered by the full-page render context — smoke-test specifically).
- **Block List** — `FromEditor` source IS the block JSON; the converter resolves element types from the published content-type cache (no parent content / `IPublishedRequest` needed). Blocks need an `Expose` entry for the relevant culture/segment to surface.

Recommended primitive: reuse `IPublishedContentFactory` rather than hand-assembling per property. Variant content must populate `PropertyData` per culture/segment + `ContentData.CultureInfos`, and read-time resolution depends on the ambient `IVariationContextAccessor`.

## Client components

| Unit | Responsibility |
|---|---|
| `UmbVisualEditorRenderController` (new sibling, follows the SignalR/router/resolver extraction pattern) | Debounce (~500ms) + latest-wins cancellation via `AbortController`. Collects the active variant's current values from the workspace, calls the endpoint, posts `umb:ve:render` to the guest with the returned HTML. On failure: keep DOM, log, transient notice. Invoked from every mutation site (property submit, block submit, add/move/delete/reorder, and the debounced optimistic text input). |
| guest `injected.ts` | Bundle **morphdom**. Refactor the one-shot init (default outlines, drag-sort setup, add-button insertion, region discovery) into a re-runnable `initRegions()`. On `umb:ve:render`: morph `document.body` to the new HTML, then run `initRegions()` and restore the selection highlight. Delegated document-level listeners (click capture, mouseover) survive the morph; per-node styles/attributes are re-applied by `initRegions()`. |
| element SignalR (`visual-editor-signalr.controller.ts` + element) | Reintroduce a short-lived **suppress-self-reload** guard set when this editor saves, so the `refreshed` event for our own document key is ignored. Refreshes outside the guard window still reload (multi-user / external cache changes). |

## Error handling

- Render failure (network/500/timeout): keep last good DOM, log, show a transient non-blocking "preview out of date" indicator. The edit is already in the workspace; a later successful render reconciles. No silent swallow.
- Latest-wins: a newer edit aborts the in-flight render so stale HTML never overwrites newer DOM.

## Out of scope

- Render caching / output pooling beyond debounce + concurrency cap.
- Headless / Delivery-API rendering in the iframe.
- Surfacing validation state in the preview.
- Server-side sub-region extraction (full-page render + client morph already delivers partial DOM updates).
- Inline (`contenteditable`) editing — that is Phase 4 and now has its server-rendered source of truth from this phase.

## Testing

- **Backend integration test** (the riskiest, and testable C#, unlike the UI surface): the spike's throwaway test at `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/PropertyEditors/VisualEditorConversionSpikeTests.cs` (uncommitted) is the basis — the plan's first task formalizes it into a real test of the override-content builder for TextBox, RTE, and Block List (assert converted-without-saving == save-then-read). A second test renders a seeded document through the render service and asserts the HTML reflects overridden values and carries `data-umb-*` annotations.
- **Frontend**: `npm run build` + `npm run lint` + manual smoke (no VE test harness exists; consistent with the prior phase).

## Spike outcome (2026-06-11) — PASSED

A throwaway integration test (`VisualEditorConversionSpikeTests`, uncommitted) booted Umbraco on SQLite, seeded a doc with TextBox + Rich Text + Block List, and proved that each property's editor-format value converts to the correct published value **without saving**, via `FromEditor` + `IPublishedContentFactory.ToIPublishedContent`. All 3 assertions passed (convert-without-saving == save-then-read). Findings folded into "Server components" above:

- Conversion primitive: `IPublishedContentFactory` (HybridCache, `internal`) — plan must resolve the access seam.
- Approach is a cache-node rebuild, **not** a `GetProperty` decorator (in-tree precedent: `BlockElementService`).
- Variants: thread `Culture`/`Segment` + `ContentData.CultureInfos`; read-time needs `IVariationContextAccessor`.
- Render-to-string is independently de-risked by the existing `TemplateRenderer`; RTE-embedded-block partials are the one spot needing the render context (not the value conversion).

No design fallback required — the approach is viable as chosen.
