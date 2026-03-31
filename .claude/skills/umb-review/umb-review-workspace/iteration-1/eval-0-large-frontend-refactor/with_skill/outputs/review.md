## PR Review

**Target:** `origin/main` · **Based on commit:** `70be022109a` · **Skipped:** 0 files out of 78 total

Migrates create entity actions for templating (partial views, scripts, templates), language, member group, and document blueprints from custom `UmbEntityActionBase` implementations to the newer `entityCreateOptionAction` extension pattern. Adds `additionalOptions: true` to several existing create option manifests and appends `...` to labels in create-options modals. Updates E2E tests and helpers accordingly.

- **Modified public API:** `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` (deprecated), `UmbDocumentBlueprintOptionsCreateModalData` (deprecated), `UmbDocumentBlueprintOptionsCreateModalValue` (deprecated) — all via `@umbraco-cms/backoffice/document-blueprint`
- **Breaking changes:** Manifest aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` are renamed to `Umb.EntityAction.PartialView.Create` and `Umb.EntityAction.Script.Create` respectively. Plugin developers referencing these old alias strings in conditions, overwrites, or extension registry lookups will silently break.
- **Other changes:** Create buttons in collection views now append `...` to labels when `additionalOptions: true` is set. Document, media, and member create-options modals append `...` to type names in their list items.

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **Formatting mixed with logic:** `UiBaseLocators.ts` contains ~1000 lines of formatting changes (single→double quotes, line wrapping) mixed with functional locator updates. Consider a separate formatting-only commit or PR to keep the functional diff reviewable.

---

### Critical

- **`src/.../partial-views/entity-actions/create/manifests.ts:12`**: Manifest alias renamed from `Umb.EntityAction.PartialView.CreateOptions` to `Umb.EntityAction.PartialView.Create`. Plugin developers reference aliases by string in conditions, overwrites, and extension registry lookups — this silently breaks those references. The old alias should be preserved as a deprecated manifest entry that maps to the same behavior, similar to how the old modal aliases (`Umb.Modal.PartialView.CreateOptions`) are kept. Same pattern applies to `Umb.EntityAction.Script.CreateOptions` in `src/.../scripts/entity-actions/create/manifests.ts:10`.

### Important

- **`src/.../scripts/entity-actions/create/manifests.ts:11`**: Missing `weight: 1200` on the `entityAction` kind `create` manifest. The old manifest had `weight: 1200` and all sibling create entity actions in this PR (language, member-group) include it. Same issue in `src/.../partial-views/entity-actions/create/manifests.ts:13`, `src/.../templates/entity-actions/manifests.ts:13`, and `src/.../document-blueprints/entity-actions/create/manifests.ts:11`. Without explicit weight, the create action may appear in a different position in the actions menu than before. → Add `weight: 1200` to match siblings.

- **`src/.../document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:28`**: Uses `history.pushState` for navigation instead of the `getHref()` pattern used by all other new create option actions (language, member-group, partial-view, script, template). Those return a URL string from `getHref()` which lets the framework render an `<a>` tag with proper semantics (ctrl+click, middle-click open in new tab). The blueprint action overrides `execute()` and pushes state imperatively, losing those benefits. → Consider refactoring to use `getHref()` for consistency with the new pattern, extracting the document-type picker into a pre-step or keeping `execute()` only for the picker flow and navigating with the href afterward.

### Suggestions

- **`src/.../core/collection/action/create/collection-create-action.element.ts:145`**: The ellipsis is appended via `label + '...'` based on `manifest.meta.additionalOptions`. This duplicates the same logic that appears in multiple create-options modals (document, media, member). Consider centralizing this — e.g., having the `create` entity action kind or the base collection action element handle it so individual manifests don't each need `additionalOptions: true` just for a UI suffix.

- **`src/.../documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:22`**: The `selection.filter((x) => x !== null)` followed by `selection[0]` can be simplified. If the filter result is empty, `documentTypeUnique` will be `undefined`, which is then caught by the `if (!documentTypeUnique)` check — but the intent would be clearer with e.g. `value.selection.find((x) => x !== null)`.

---

## Request Changes

Critical and important issues must be addressed first.
