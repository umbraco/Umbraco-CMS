# Visual Editor — Framework-Emitted Empty-Block Affordance — Design

**Status**: Implemented
**Date**: 2026-06-12
**Author**: Rick Butterfield + Claude
**Scope**: Move the "empty editable block property" visual-editor affordance (the annotated container that lets the guest offer an "Add content" button) out of per-view template code and into the framework block-rendering helpers, so it works automatically for every template — including custom ones — with zero template boilerplate.
**Supersedes**: the per-view empty-state edits to `blockgrid/blocklist/singleblock/default.cshtml` (sample site) and `EmbeddedResources/BlockGrid/default.cshtml`, plus the `PropertyAliasViewDataKey` ViewData plumbing in `BlockListTemplateExtensions`/`BlockGridTemplateExtensions`.

---

## Problem

The visual editor needs a DOM anchor for empty, editable block properties so the guest can render an "Add content" affordance (it has no blocks to attach inter-block "+" buttons to). The current implementation puts this in the Razor templates:

- `GetBlock{List,Grid}HtmlAsync` short-circuits empty models to `HtmlString.Empty`.
- Each `default.cshtml` was patched to read a `PropertyAliasViewDataKey` from ViewData and render an annotated empty `<div ... data-umb-block-property="{alias}">` in visual-editor mode.

This is unfriendly and incomplete:
- Every block template (block list, block grid, single block — default **and** any custom template) must carry framework annotation boilerplate.
- Custom templates that don't include it silently lose the feature.
- It contrasts with regular property annotation, which is fully automatic (`UmbracoViewPage` wraps editable property output in `data-umb-property` spans with no template code).

## Goal

Make the empty-block affordance **fully automatic**: no template code, working for the default templates and any custom template, gated on the property's `EditableInVisualEditor` opt-in and on visual-editor/preview mode. Revert all per-view edits and the ViewData plumbing.

## Why not the obvious alternatives

- **Emit it from `UmbracoViewPage` (like `data-umb-property`)**: the automatic span is only emitted when the property is accessed via the tracked `IPublishedContent.Value()` path; the block helpers read the value via `GetProperty().GetValue()`, which bypasses the tracker. Making block access reliably tracked and anchoring an affordance on an empty span touches the core annotation pipeline — bigger and riskier (this is the deferred "unify all property annotation" direction).
- **Emit HTML from the Core block model**: `BlockListModel`/`BlockGridModel` live in `Umbraco.Core`, which has no web/HTML concern — emitting annotation markup from the model crosses a layer boundary.

## Approach (chosen)

The block-rendering helpers in `Umbraco.Web.Common` are the web-layer choke point essentially all block rendering flows through. Move the empty-state emission there.

### Component 1 — Helpers emit the annotated container

In `BlockListTemplateExtensions`, `BlockGridTemplateExtensions`, and the single-block rendering helper:

- When the model is **empty** AND `VisualEditorPropertyTracker.IsEnabled` AND the property's `PropertyType.EditableInVisualEditor` is `true`, return a minimal annotated container as an `HtmlString`:
  - Block list: `<div class="umb-block-list" data-umb-block-property="{alias}"></div>`
  - Block grid: `<div class="umb-block-grid" data-umb-block-property="{alias}"></div>` (with the existing `data-grid-columns`/`--umb-block-grid--grid-columns` styling, defaulting columns to `12`)
  - Single block: an analogous annotated empty container (see Component 3)
- Otherwise return `HtmlString.Empty` exactly as today. Non-empty models render their partial unchanged.

The helper builds this small fixed container directly (no partial, no ViewData). The `PropertyAliasViewDataKey` constant, the `WithPropertyAlias` helper, and the alias-via-ViewData private overloads are **removed** from both extensions.

Gating predicate (shared intent across all three helpers): `model is empty && VisualEditorPropertyTracker.IsEnabled && propertyType?.EditableInVisualEditor == true`.

### Component 2 — Emission lives in the alias-bearing overloads only (no model metadata)

The helpers have three call styles:

| Overload | Has alias + editable flag? |
|---|---|
| `GetBlock*HtmlAsync(IPublishedContent content, string alias[, template])` | Yes — resolves the `IPublishedProperty` (`alias`, `PropertyType.EditableInVisualEditor`) |
| `GetBlock*HtmlAsync(IPublishedProperty property[, template])` | Yes — `property.Alias`, `property.PropertyType.EditableInVisualEditor` |
| `GetBlock*HtmlAsync(BlockListModel/BlockGridModel model[, template])` | **No** |

The empty-state container is emitted **only by the two alias-bearing overloads**, because they carry the alias and editable flag regardless of whether the value is empty.

**Why not "alias on the model" (rejected):** empty block values resolve to a process-wide **singleton** — the value creators return `BlockListModel.Empty` / `BlockGridModel.Empty` (`public static`), and an empty single block converts to `null`. There is no per-property instance to carry an alias for the empty case, and setting a mutable alias on the shared singleton would corrupt every empty block property on the site. The alias is also unavailable where the model is built (the value *creators* don't receive `IPublishedPropertyType` — only the *converters* do). So model metadata is out; **no changes to Core models, value creators, or converters.**

**Consequence for the model-only overload:** `GetBlock*HtmlAsync(Model.BlockProperty)` (model-only, including the bare ModelsBuilder property) keeps its current behaviour — empty renders nothing, no affordance. The alias-bearing overload (`GetBlock*HtmlAsync(Model, "alias")` / `(IPublishedProperty)`) is the documented, default pattern used by all sample templates (and `Home.cshtml` was aligned to it), so "fully automatic" holds for the standard pattern. The model-only gap is in the same class as fully hand-rolled rendering — see Out of scope.

### Component 3 — Single block

The single-block helper is `SingleBlockTemplateExtensions.GetBlockHtmlAsync`; an empty single-block property surfaces as a **null** `BlockListItem` (the helper already returns `HtmlString.Empty` for null). Emit an annotated empty container when the value is null/empty + `VisualEditorPropertyTracker.IsEnabled` + the property is `EditableInVisualEditor`.

Consistent with Component 2: annotation comes only from the **alias-bearing overloads** — `GetBlockHtmlAsync(IPublishedProperty)` and `GetBlockHtmlAsync(IPublishedContent, alias)` — which expose `property.Alias` and `property.PropertyType.EditableInVisualEditor` even when `property.GetValue()` is null. The model-only `GetBlockHtmlAsync(BlockListItem? model)` overload, given a null model, has no alias and cannot annotate (documented gap; the sample/default and documented usage use the alias-bearing overloads).

"Add content" reuses the existing `umb:ve:block-add-to-property` message (single-block semantics: one block, `insertIndex 0`). The guest gains a single-block empty-container branch mirroring the list/grid ones (or a shared selector). Exact container markup + the guest branch are finalized in the plan.

### Component 4 — Guest + element (mostly unchanged)

- The guest already attaches the "Add content" placeholder to empty `.umb-block-list` / `.umb-block-grid` containers carrying `data-umb-block-property`, and the element's grid-aware add (`#resolveBlockSchemaAlias`) already produces the correct list/grid value shape. These are unchanged.
- The only guest addition is the single-block empty-container handling.
- The `data-umb-block-property` attribute and the `umb:ve:block-add-to-property` postMessage protocol are retained — the helper now emits the attribute that the templates previously emitted.

### Component 5 — Revert the per-view changes

Revert to original form (removing the empty-state boilerplate and ViewData reads):
- `src/Umbraco.Web.UI/Views/Partials/blockgrid/default.cshtml`
- `src/Umbraco.Web.UI/Views/Partials/blocklist/default.cshtml`
- `src/Umbraco.Web.UI/Views/Partials/singleblock/default.cshtml` (unchanged from original — never modified, but confirm it needs no edit under the new mechanism)
- `src/Umbraco.Core/EmbeddedResources/BlockGrid/default.cshtml`

`Home.cshtml`'s switch to the alias-aware overload (`GetBlockGridHtmlAsync(Model, "bodyText")`) may be **kept or reverted** — under Component 2 the model-only overload also works, so reverting it is safe; keeping it is harmless. The plan picks one (default: keep, as the alias-aware overload is the documented norm).

## Data flow (after)

```
template: @await Html.GetBlockGridHtmlAsync(Model, "bodyText")   (or Model.BodyText, or an IPublishedProperty)
  → helper resolves model + property alias + EditableInVisualEditor
  → model non-empty?  → render partial as today (unchanged)
  → model empty?
       → VisualEditorPropertyTracker.IsEnabled && EditableInVisualEditor?
            → return <div class="umb-block-grid" data-umb-block-property="bodyText"></div>
            → else HtmlString.Empty   (production: nothing, as today)
  → guest sees the empty annotated container → renders "Add content" → umb:ve:block-add-to-property
  → element #onBlockAddToProperty → grid/list-aware value creation (unchanged)
```

## Error handling / edge cases

- Not in VE/preview, or property not editable, or model non-empty → byte-for-byte the same output as before this change (no behavioural change to production rendering).
- Property alias unknown on the model-only overload (metadata not populated, e.g. a model constructed outside the value creators) → no annotation (graceful: treated as "alias unknown", returns empty as today). Not silent in a harmful way — it just falls back to current behaviour.
- A custom template that hand-renders blocks without any `GetBlock*HtmlAsync` helper → no affordance. Documented as the one uncovered path (the helper is the documented rendering API).

## Testing

- **Backend (unit/integration)**: the block helpers return an annotated container for an empty editable block property when `VisualEditorPropertyTracker.IsEnabled`, and `HtmlString.Empty` when (a) the tracker is disabled, (b) the property is not `EditableInVisualEditor`, or (c) the model is non-empty. Cover all three overloads (content+alias, property, model-only) — the model-only case asserts the `PropertyAlias` metadata path.
- **Value-creator test**: the produced block model carries the correct `PropertyAlias` / `EditableInVisualEditor` metadata.
- **Frontend/guest**: `npm run build` + `npm run lint` + manual smoke (no VE guest test harness; consistent with the feature's established posture). Manual smoke covers empty list, empty grid, empty single block in the visual editor.

## Out of scope

- Unifying all property annotation under a single `data-umb-property` mechanism (the deferred Approach 2).
- Shipping a default block-list render template (block list intentionally ships none).
- Covering hand-rolled block rendering that bypasses the `GetBlock*HtmlAsync` helpers.
- Covering the **model-only** helper overload (`GetBlock*HtmlAsync(Model.BlockProperty)`): empty values resolve to the shared `.Empty` singleton (or `null` for single block), which has no per-property identity to annotate. Use the alias-bearing overload (`GetBlock*HtmlAsync(Model, "alias")`) — the documented default — to get the empty-state affordance.

## Implementation note

This change **reverts** the prior per-view empty-state commits and the ViewData plumbing in favour of the helper-based mechanism. Those commits remain in history as superseded steps; the revert is part of this work, not a separate cleanup.
