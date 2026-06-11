# Visual Editor — Partial Re-render (Phase 3 remainder) — Design

**Status**: Approved design; **gated on conversion spike** (must pass before the full implementation plan is written)
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
| `PropertyOverridePublishedContent` | `Umbraco.Web.Common` | `IPublishedContent` decorator. For each overridden alias it returns an override `IPublishedProperty` whose value is produced by running the editor-format value through `IDataValueEditor.FromEditor` → the property type's `IPropertyValueConverter` (the conversion path proven by the BlockPreview community package, §2.3). All non-overridden members delegate to the inner draft content. Variant-aware (override applies to the requested culture/segment). |
| `IVisualEditorRenderService` + impl | `Umbraco.Web.Common` | Renders a supplied `IPublishedContent` to an HTML string. Modeled on `TemplateRenderer` (`src/Umbraco.Web.Common/Templates/TemplateRenderer.cs`): build an `IPublishedRequest` via `IPublishedRouter`, `SetPublishedContent(overriddenContent)`, set culture/segment + template, swap onto `UmbracoContext.PublishedRequest`, render the template view to a `StringWriter`, restore. Forces preview mode + enables `VisualEditorPropertyTracker` for the render scope so annotations are emitted. |
| `RenderVisualEditorController` | `Umbraco.Cms.Api.Management` | `POST /umbraco/management/api/v1/visual-editor/render`, `[Authorize(Policy = BackOfficeAccess)]`. Ensures an `UmbracoContext`, resolves the draft content for `unique`, builds the decorator from the request `values`, calls the render service, returns `{ html }`. |

**Highest risk — the value conversion.** Turning the workspace's editor-format value (block JSON, RTE HTML, media key/UDI, plain string) into the published value a template expects is the crux. It mirrors BlockPreview's `BlockDataConverter` (`FromEditor` + value converter). This is gated by a spike (below) before the full plan is written.

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

- **Backend integration test** (the riskiest, and testable C#, unlike the UI surface): in `Umbraco.Tests.Integration`, render a seeded document through the render service with `PropertyOverridePublishedContent` overrides for a TextBox, an RTE, and a Block List property; assert the rendered HTML reflects the overridden values and carries the expected `data-umb-*` annotations.
- **Frontend**: `npm run build` + `npm run lint` + manual smoke (no VE test harness exists; consistent with the prior phase).

## Spike gate (must pass before the full implementation plan)

Before writing the multi-task plan, validate the conversion + render feasibility as a standalone, throwaway investigation (most cheaply as a rough integration test in `Umbraco.Tests.Integration` that boots Umbraco on SQLite):

1. Construct/seed a document with a TextBox, an RTE, and a Block List property and an assigned template.
2. Build a first-cut `PropertyOverridePublishedContent` that overrides each with an editor-format value via `FromEditor` + the property value converter.
3. Render the template to a string via a render-service prototype (clone of `TemplateRenderer`) in preview mode with the tracker enabled.
4. Assert the output reflects the overridden values for all three property kinds and contains `data-umb-property` / `data-umb-block-key` annotations.

**Report the spike result before committing to the full plan.** If the conversion path proves materially harder than BlockPreview's (e.g. block context resolution, variant edge cases), revise the design (fallback options: isolated-block render for blocks only, or accept save-and-reload for blocks while properties use re-render).
