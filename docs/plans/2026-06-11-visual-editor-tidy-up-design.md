# Visual Editor Tidy-Up — Design

**Status**: Approved design, pending implementation
**Date**: 2026-06-11
**Author**: Rick Butterfield + Claude
**Scope**: Tidy-up round on `feature/visual-editor` after merging `main` — no new feature phases.
**Relates to**: `docs/plans/visual-page-builder.md` (the feature plan; updated as part of this round)

---

## Decisions locked in this round

| Decision | Outcome |
|---|---|
| Architecture | **Embedded document-workspace view** is the current direction. The standalone-window evolution (plan doc §10, Open Q11) is **deferred**, not the next step. |
| Round scope | **Tidy-up only** — security, semantics, refactor, docs. No partial re-render API, no inline editing. |
| Editability semantics | **Strict opt-in everywhere** for document properties: a property is annotated/editable only when `appearance.editableInVisualEditor === true`. The frontend opt-out fallback is removed. |
| Block modal properties | **No filter**: the block editing modal shows all of the element type's content/settings properties. The `EditableInVisualEditor` setting governs document property annotation only. |

## Why

The branch is functionally far ahead of its plan doc (Phases 1–2 complete plus most block manipulation), but an audit found:

1. **Security**: the guest script (`src/Umbraco.Web.UI.Client/src/apps/visual-editor/injected.ts:540`) accepts `message` events with no `evt.origin` check, and posts with target `'*'`. The backoffice-side listener also lacks source/origin validation.
2. **Semantic mismatch**: backend tracking is strict opt-in (`PublishedContentExtensions.TrackVisualEditorAccess` checks `EditableInVisualEditor`), while the frontend had a conflicting "if none opt in, include all" fallback — dead code for properties, but confusing and wrong.
3. **Maintainability**: `document-workspace-view-visual-editor.element.ts` is 1,210 lines with ~11 responsibilities.
4. **Gap**: root-level empty Block Lists cannot offer "Add content" (container lacks a property-alias annotation; `injected.ts:1040` TODO).
5. **Stale docs**: `visual-page-builder.md` predates the `EditableInVisualEditor` setting and records "standalone window" as decided.

## Changes

### 1. Security hardening

**Guest script** (`injected.ts`):
- Derive `PARENT_ORIGIN` once: `document.referrer ? new URL(document.referrer).origin : window.location.origin`.
- Incoming handler: drop messages where `evt.origin !== PARENT_ORIGIN`.
- Outgoing: `window.parent.postMessage(msg, PARENT_ORIGIN)` instead of `'*'`.
- Referrer-based derivation keeps cross-origin dev (Vite 5173 → server 44339) working.

**Workspace view element**: the `message` listener accepts only events where `evt.source === iframe.contentWindow` **and** `evt.origin` equals the server origin from `UMB_SERVER_CONTEXT`.

### 2. Strict opt-in semantics

In the element's property-structure resolution:
- Remove the `anyExplicitlyEnabled` hybrid; filter document properties to `appearance.editableInVisualEditor === true` only.
- Remove the filter entirely from block content/settings structure resolution (blocks show all fields).
- Replace `as { editableInVisualEditor?: boolean }` casts with the typed `UmbPropertyTypeAppearanceModel`.

Backend is already strict — no backend change.

### 3. Element refactor (extraction-only)

Extract from `document-workspace-view-visual-editor.element.ts` into sibling files; no behavior change:

| New file | Responsibility |
|---|---|
| `visual-editor-signalr.controller.ts` | `HubConnection` lifecycle, `refreshed` event, refresh-suppression guard |
| `visual-editor-property-structure.resolver.ts` | Document/block/settings property-structure resolution incl. composition-chain fetch and caching; `Map`-indexed by alias (replaces 6× O(n) `find()`); sole home of the opt-in filter |
| `visual-editor-message-router.ts` | Typed message-map routing of guest messages (replaces 7-case switch); performs the origin/source validation from §1 |

The element keeps iframe lifecycle, modal registrations, selection state and preview URL — target ≤ ~600 lines. Also: `Object.keys(pastedBlocks.layout)[0]` → `Object.values(pastedBlocks.layout)[0]` (line 972).

### 4. Root-level empty block lists

- `BlockListTemplateExtensions` passes the property alias to the partial via `ViewData` (alias-aware overloads; empty models no longer short-circuit so the partial can render an annotated empty container in preview mode).
- `Views/Partials/blocklist/default.cshtml` emits `data-umb-block-property="<alias>"` on the list container — a distinct attribute, because `data-umb-property` is the guest script's property-region selector and would turn the whole list into a clickable property region.
- `injected.ts` resolves the alias from the container for empty root-level lists and renders the existing "Add content" button via a new `umb:ve:block-add-to-property` message (closes the `injected.ts:1040` TODO).

### 5. Docs & polish

- XML docs: class-level summary on `VisualEditorPropertyTracker`; `<param>` tags on `VisualEditorGuestScript.GetScriptTag()`.
- `docs/plans/visual-page-builder.md`: refresh status header and phase statuses; close Open Q5 (setting shipped, strict opt-in); mark §10/Q11 standalone window **Deferred** with embedded view as current; update Appendix B attribute table.

## Out of scope

- Partial re-render API (Phase 3 remainder), inline editing (Phase 4), headless rendering, validation surfaced in preview, scroll retention.
- Moving the visual editor to its own package / lifting block-manipulation logic to library level — revisit with Phase 3.
- Automated tests for the visual editor (no harness exists for this surface yet; E2E coverage noted in the plan doc as future work).

## Verification

1. `npm run build` and `npm run lint` in `src/Umbraco.Web.UI.Client`.
2. `dotnet build umbraco.sln` — zero errors, no new warnings.
3. Manual smoke in the visual editor tab: property edit (flagged + unflagged property), block add/edit/settings/move/delete, empty root-level block list "Add content", save → SignalR refresh → selection restore, postMessage still works in dev (Vite) and built modes.
