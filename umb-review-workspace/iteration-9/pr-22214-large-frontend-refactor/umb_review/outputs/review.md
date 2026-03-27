## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25b3897f5e592e11e0b6eaeaa4bdd8` · **Skipped:** 0 noise files out of 78 total

Migrates create entity actions for Templating (partial views, scripts, templates), Language, Member Group, and Document Blueprints from monolithic modal-based `default`-kind entity actions to the `entityCreateOptionAction` extension system, enabling plugins to register additional create options for these entity types. Deprecated old action classes and modal tokens are retained with `UmbDeprecation` warnings.

- **Modified public API:** `Umb.EntityAction.PartialView.CreateOptions` alias renamed to `Umb.EntityAction.PartialView.Create`; `Umb.EntityAction.Script.CreateOptions` renamed to `Umb.EntityAction.Script.Create`; `Umb.EntityAction.Template.Create`, `Umb.EntityAction.Language.Create`, `Umb.EntityAction.MemberGroup.Create`, `Umb.EntityAction.DocumentBlueprint.Create` — kind changed from `default` to `create` (no alias change on the last four). Old modal tokens `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`, `UMB_SCRIPT_CREATE_OPTIONS_MODAL`, `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` marked deprecated.
- **Breaking changes:** Two entity action alias renames (see Critical section).
- **Other changes:** Labels in create-options modals for Documents, Media, Members now append `...` to item names. Collection create actions added for Partial View, Script, and Template tree-item-children collections. New `UMB_CREATE_*_WORKSPACE_PATH_PATTERN` path pattern exports for member-group, partial views, scripts, and templates.

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **Formatting mixed with logic:** `UiBaseLocators.ts` — full diff is 1065 lines, whitespace-ignored diff is 1013 lines (only ~5% formatting). This threshold is not a concern.
> - **Size:** 78 files changed across Frontend (backoffice) and Test layers spanning several packages. No production layer split needed — all changes are Frontend-only, so a single PR is reasonable.
>
> _This is an observation, not a blocker. The full review follows below._

---

### Critical

- **`src/packages/templating/partial-views/entity-actions/create/manifests.ts` (entire file)**: The entity action alias `Umb.EntityAction.PartialView.CreateOptions` is renamed to `Umb.EntityAction.PartialView.Create`. Alias strings are not caught by the compiler — any plugin that references the old alias in conditions, overwrites, or extension registry lookups (`extensionRegistry.getByAlias('Umb.EntityAction.PartialView.CreateOptions')`) will silently break at runtime. Per the manifest/extension system breaking-change rules, the old alias must be preserved as a deprecated parallel entry pointing to the same implementation. Add a second manifest entry with the old alias and mark it deprecated, or confirm via changelog/migration docs that this intentional rename is acceptable as a public API break.

- **`src/packages/templating/scripts/entity-actions/create/manifests.ts` (entire file)**: Same issue — alias `Umb.EntityAction.Script.CreateOptions` → `Umb.EntityAction.Script.Create` is a breaking rename for any plugin or condition referencing the old alias string.

---

### Important

- **`src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:41`**: Uses `history.pushState(null, '', url)` for navigation after document-type selection. This is a raw browser API call rather than the router abstraction used in other create-option actions (`UmbEntityCreateOptionActionBase.getHref()` + href-based routing). This inconsistency means router-aware code (e.g. history stack, hash-change listeners) won't fire. The other new create-option actions (`default-partial-view-`, `default-script-`, `default-template-`, `default-language-`, `default-member-group-`) correctly use `getHref()`. Consider overriding `getHref()` here instead of `execute()` + imperative navigation, for consistency and router compatibility.

- **`src/packages/templating/partial-views/entity-actions/create/options-modal/index.ts`** and **`src/packages/templating/scripts/entity-actions/create/options-modal/index.ts`**: The deprecated `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL` and `UMB_SCRIPT_CREATE_OPTIONS_MODAL` tokens are still exported from their `index.ts` files. If these `index.ts` files are re-exported through a package-level barrel that is reachable via the `package.json` exports field, the deprecated tokens remain part of the public API (good), but callers would then need a migration path. Confirm the barrel chain: `constants.ts` re-exports `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`. This is fine as-is if it exists in the export tree — but verify that these tokens (and the `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` in `constants.ts`) are still reachable to external consumers who may hold references, otherwise they'd get a runtime "modal not found" error with no compile-time warning.

- **`src/packages/templating/partial-views/entity-actions/create/manifests.ts:8`**: After the rename, the old alias `Umb.EntityAction.PartialView.CreateOptions` appears in the PR's file (in the pre-merge `origin/main` copy) as a deprecated inline comment `// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.` on the modal registration, but there is NO corresponding deprecated `entityAction` entry for the old alias. Only the modal (`Umb.Modal.PartialView.CreateOptions`) is kept for backwards compatibility — the entity action alias itself is not preserved. Same for `Umb.EntityAction.Script.CreateOptions`.

---

### Suggestions

- **`src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:178`**, **`media-create-options-modal.element.ts:103`**, **`member-create-options-modal.element.ts:72`**: The ellipsis `...` is appended via string concatenation (`.name + '...'`). The locale and font may render a different ellipsis character than what `additionalOptions: true` triggers in `collection-create-action.element.ts` (where it appends `'...'`). If a shared helper or constant exists for the ellipsis string, use it for consistency. Minor, but worth checking.

- **`src/packages/language/entity-actions/create/default/manifests.ts` and `members/member-group/entity-actions/create/default/manifests.ts`**: Both new `entityCreateOptionAction` manifests include `additionalOptions: true` in their meta, implying more options are expected beyond this one. Since Language and Member Group currently only have a single create option (no folder, no "from snippet"), the ellipsis label suggests there are further options to navigate to — but the action navigates directly to the workspace via `getHref()`. This will cause a `...` to appear on a single, non-branching option. Intended behavior or oversight?

- **`src/packages/documents/document-blueprints/entity-actions/create/default/manifests.ts:17`**: The `label` is a plain string `'Document Blueprint for'` (not a localization key). Other entries in this PR use `#create_*` localization keys. Consider using a proper localization key for consistency and translatability.

---

## Request Changes

Critical and important issues must be addressed first.
