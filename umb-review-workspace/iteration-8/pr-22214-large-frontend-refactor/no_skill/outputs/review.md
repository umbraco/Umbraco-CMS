# Code Review: PR #22214 — Migrate Create Entity Actions to entityCreateOptionAction Extensions

**PR Title**: Backoffice: Migrate Templating, Language, Member Group, and Document Blueprint create entity actions to use entityCreateOptionAction extensions
**Branch**: `v17/feature/templating-create-action-options` → `main`
**Files changed**: 78
**Lines**: +1,323 / -566

---

## Summary

This PR migrates several entity types from bespoke "create options" modal-based flows to the shared `entityCreateOptionAction` extension system. The entities migrated are: Templates, Partial Views, Scripts, Document Blueprints, Languages, and Member Groups. It also adds `additionalOptions: true` metadata to several already-migrated entities (Data Types, Document Types, Media Types, Member Types, Stylesheets), adds collection create actions to tree item children collections for new entity types, and appends ellipsis (`...`) to option labels throughout the relevant modals to signal that more steps follow.

The change is largely mechanical and consistent, following the pattern already established for Documents and Media. The overall direction is sound.

---

## Findings

### 1. Bug — Blueprint Label Not Localised

**File**: `src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/manifests.ts`

```ts
meta: {
    icon: 'icon-blueprint',
    label: 'Document Blueprint for',   // ← hardcoded English string
    additionalOptions: true,
},
```

Every other create option action in this PR uses a localisation key (e.g. `'#create_newTemplate'`, `'#actions_create'`). This label is an unwrapped English string with an odd trailing `"for"`, which will appear literally in non-English locales. A proper localisation key should be introduced (e.g. `'#create_documentBlueprintFor'` or similar) and the corresponding string added to the language files. The label wording itself is also incomplete and reads oddly.

---

### 2. Bug — Template `entityCreateOptionAction` Not Gated by Production Mode Condition

**File**: `src/Umbraco.Web.UI.Client/src/packages/templating/templates/entity-actions/manifests.ts`

The parent `entityAction` of kind `'create'` correctly carries the `UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS` condition so it is hidden in production:

```ts
conditions: [{ alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS, match: false }],
```

However, the child `entityCreateOptionAction` defined in `create/default/manifests.ts` carries **no condition at all**. If the create button/modal is somehow opened (e.g. via a direct URL or another code path), the option would be accessible even in production. The `Umb.EntityCreateOptionAction.Template.Default` manifest should also carry `match: false` for the production mode condition to remain consistent with the parent.

The same concern applies to Partial Views (`Umb.EntityAction.PartialView.Create` is production-gated but `Umb.EntityCreateOptionAction.PartialView.Default` and `Umb.EntityCreateOptionAction.PartialView.FromSnippet` are not).

---

### 3. Bug — Scripts `entityCreateOptionAction` Also Missing Production Mode Condition

**File**: `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/default/manifests.ts` and `scripts/entity-actions/create/folder/manifests.ts`

Scripts in the old code were not explicitly production-gated (this matches main), but the from-snippet and folder options for partial views carry no condition either. This is a consistency issue that could lead to UI elements being exposed where they should be hidden.

---

### 4. Architectural Issue — Condition Not Propagated to `entityCreateOptionAction` for Partial Views

**File**: `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts`

The outer `entityAction` of kind `'create'` has:

```ts
conditions: [{ alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS, match: false }],
```

But neither `Umb.EntityCreateOptionAction.PartialView.Default` nor `Umb.EntityCreateOptionAction.PartialView.FromSnippet` in their respective manifests carry this condition. This means the options could potentially be registered independently and surfaced through collection create actions in production mode. The condition should be duplicated on each child `entityCreateOptionAction`.

---

### 5. Potential Bug — `UmbCreateMemberGroupEntityAction` Deleted Without Deprecation Warning

**File deleted**: `src/Umbraco.Web.UI.Client/src/packages/members/member-group/entity-actions/create-member-group.action.ts`
**File deleted**: `src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/language-create-entity-action.ts`

Unlike the Document Blueprint and Partial View cases (where the old modal/action is kept with deprecation notices), the `UmbCreateMemberGroupEntityAction` and `UmbLanguageCreateEntityAction` classes are deleted outright. If any external code (e.g., third-party packages) imported or instantiated these classes directly, they will break silently at runtime. The project's own guidelines state that breaking changes within a major version should be avoided by preserving the old exports as deprecated stubs. The files should be retained, marked `@deprecated`, and re-export or delegate to the new implementation.

---

### 6. Code Quality — `additionalOptions: true` on Folder Actions Is Misleading

**Files**: Every `create/folder/manifests.ts` introduced in this PR (Document Blueprint, Partial View, Script)

```ts
meta: {
    icon: 'icon-folder',
    label: '#create_folder',
    additionalOptions: true,
    folderRepositoryAlias: ...,
},
```

Folder creation via the `'folder'` kind already shows a prompt for a folder name inline. Setting `additionalOptions: true` appends `...` to the label (e.g. "Folder...") which implies that clicking the option opens further sub-options. The actual UX is a single name-input dialog, which is not "additional options" in the same sense. This causes visual inconsistency vs. options where `additionalOptions` is used to indicate a doc-type picker or snippet modal. The flag should be removed from folder options, or the rendering logic should distinguish between "further picker" and "inline prompt".

---

### 7. Code Quality — Double Assignment of `createModalBtn` in `UiBaseLocators.ts`

**File**: `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`

```ts
this.createModalBtn = page.locator("uui-modal-sidebar").getByLabel("Create", { exact: true });
this.createModalBtn = this.sidebarModal.getByLabel("Create", { exact: true });
```

The field is assigned twice in sequence. This was present in the original code but the formatting refactor in this PR preserves the double assignment. The first assignment is immediately overwritten; it is dead code and should be removed.

---

### 8. Code Quality — `selection[0]` Without Range Check in Blueprint Action

**File**: `src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts`

```ts
const selection = value.selection.filter((x) => x !== null);
const documentTypeUnique = selection[0];

if (!documentTypeUnique) {
    throw new Error('Document type unique is not available');
}
```

Accessing `selection[0]` when the filtered array might be empty is safe here (the `if (!documentTypeUnique)` guard catches it), but only because `undefined` is falsy. If the picker ever allows an empty string unique, the check would silently skip. More robust would be `if (!selection.length)` followed by using `selection[0]` with confidence, or using optional chaining and a more descriptive guard condition.

---

### 9. Code Quality — Inconsistent `weight` on `entityAction` of kind `'create'`

**Files**: `language/entity-actions/create/manifests.ts`, `member-group/entity-actions/create/manifests.ts`

Both new `entityAction` manifests of kind `'create'` specify `weight: 1200`, while the newly created Document Blueprint and Templates manifests do **not** include a weight. In the old code, Documents and Media already used `kind: 'create'` without an explicit weight. While the `'create'` kind probably provides a default weight, the inconsistency between entity types could lead to ordering surprises. All `kind: 'create'` actions should either all set an explicit weight or all rely on the default.

---

### 10. Code Quality — `manifests.ts` Files Declaring `Array<UmbExtensionManifest>` Instead of `Array<UmbExtensionManifest | UmbExtensionManifestKind>`

**Files**: `language/entity-actions/create/manifests.ts`, `member-group/entity-actions/create/manifests.ts`, `language/entity-actions/create/default/manifests.ts`, `member-group/entity-actions/create/default/manifests.ts`

These manifest arrays are typed as `Array<UmbExtensionManifest>` even though they spread into parent arrays typed as `Array<UmbExtensionManifest | UmbExtensionManifestKind>`. The new Document Blueprint, Partial View, Script, and Template manifests all correctly use the wider union type. The language and member-group manifests should be updated for consistency and to avoid potential type mismatches if kind-based manifests are ever added.

---

### 11. Minor — Deprecated `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL` Still References a Non-Exported Interface

**File**: `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/options-modal/index.ts`

The `UmbPartialViewCreateOptionsModalData` interface was deprecated with a JSDoc comment but no corresponding `@deprecated` is added to the modal token instantiation or index barrel export. Compare with the script variant, where `UmbScriptCreateOptionsModalData` (in `types.ts`) also received the `@deprecated` tag. For consistency, the deprecation markup in the partial views `index.ts` should mark both the interface and the token.

---

### 12. Minor — Production Mode Condition Missing on `PartialView.FromSnippet` Option

**File**: `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/from-snippet/manifests.ts`

`Umb.EntityCreateOptionAction.PartialView.FromSnippet` has no condition, but "Create from snippet" only makes sense in non-production mode (the original create flow was behind the same production gate as the parent action). This option could surface on its own through collection create actions registered against production environments.

---

## Summary Table

| # | Severity | Category | Location |
|---|----------|----------|----------|
| 1 | Bug | Hardcoded/missing localisation | `document-blueprints/create/default/manifests.ts` |
| 2 | Bug | Production condition not propagated | `templates/entity-actions/manifests.ts` + `create/default/manifests.ts` |
| 3 | Bug | Production condition not propagated | `partial-views/create/default/manifests.ts` + `from-snippet/manifests.ts` |
| 4 | Architectural | Condition isolation between parent/child manifests | Partial views and Templates |
| 5 | Breaking Change | Classes deleted without deprecation stub | `create-member-group.action.ts`, `language-create-entity-action.ts` |
| 6 | Code Quality | Misleading `additionalOptions: true` on folder actions | All `create/folder/manifests.ts` files |
| 7 | Code Quality | Dead code: double assignment | `UiBaseLocators.ts` |
| 8 | Code Quality | Fragile array indexing in blueprint action | `default-blueprint-create-option-action.ts` |
| 9 | Code Quality | Inconsistent `weight` across `kind: 'create'` actions | Language and Member Group manifests |
| 10 | Code Quality | Narrow array type on manifest exports | Language + Member Group manifest files |
| 11 | Minor | Incomplete deprecation annotation | `partial-views/options-modal/index.ts` |
| 12 | Minor | Production gate missing on FromSnippet option | `partial-views/create/from-snippet/manifests.ts` |

---

## Positive Observations

- The core refactor pattern is clean and consistent: every entity now follows the same structure (`kind: 'create'` parent action + `entityCreateOptionAction` children), matching the already-established Document/Media pattern.
- Deprecation warnings (`UmbDeprecation`) are correctly added to the Document Blueprint and Partial View (and Script) legacy modal elements, giving developers runtime feedback.
- Legacy modals are preserved with `@deprecated` JSDoc and kept in the manifest registry (with appropriate comments), maintaining backward compatibility for external consumers in those cases.
- Adding `UMB_CREATE_*_WORKSPACE_PATH_PATTERN` path helpers replaces the previous hardcoded URL strings, which directly addresses the `// TODO: Lets avoid having such hardcoded URLs` comments from the old code.
- The `UiBaseLocators.ts` formatting refactor is a non-functional but welcome improvement to readability, and is the bulk of the test file changes.
- The `LanguageUiHelper.ts` fix adding `exact: true` to the Languages link locator is a useful robustness improvement.
- The test helper updates (switching from `umb-partial-view-create-options-modal` to `umb-entity-create-option-action-list-modal`) correctly track the changed component structure.
