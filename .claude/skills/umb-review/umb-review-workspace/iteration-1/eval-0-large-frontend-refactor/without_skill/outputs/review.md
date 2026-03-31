# Code Review: PR #22214 - Migrate Create Entity Actions to `entityCreateOptionAction` Extensions

**PR Title:** Backoffice: Migrate Templating, Language, Member Group, and Document Blueprint create entity actions to use entityCreateOptionAction extensions

**Reviewer:** Claude Opus 4.6 (automated)
**Commit:** `70be022109a`
**Files changed:** 78 (1,296 insertions, 539 deletions ignoring whitespace)

---

## 1. Summary of Intent

This PR migrates several entity "create" actions from ad-hoc implementations (`kind: 'default'` with custom action classes) to the standardized `kind: 'create'` entity action pattern with `entityCreateOptionAction` extensions. The affected entity types are:

- **Document Blueprints** - custom action class replaced with `entityCreateOptionAction` + folder option
- **Language** - custom action class replaced with `entityCreateOptionAction`
- **Member Group** - custom action class replaced with `entityCreateOptionAction`
- **Partial Views** - custom modal-based action replaced with `entityCreateOptionAction` + from-snippet + folder options
- **Scripts** - custom modal-based action replaced with `entityCreateOptionAction` + folder options
- **Templates** - custom action class replaced with `entityCreateOptionAction`

Additionally, it adds `additionalOptions: true` to several existing `entityCreateOptionAction` manifests (data types, document types, media types, member types, stylesheets) and appends ellipsis (`...`) to labels in create options modals for documents, media, and members.

The PR also adds `collectionAction` (kind: `create`) manifests to tree-item-children collections for document blueprints, partial views, scripts, and templates.

A large portion of the diff (~750 lines) is a reformatting of `UiBaseLocators.ts` (single quotes to double quotes, line wrapping) in the acceptance tests.

---

## 2. Architecture & Pattern Compliance

### Severity: Info - Follows Established Pattern

The migration follows the exact pattern already established by stylesheets (see `src/Umbraco.Web.UI.Client/src/packages/templating/stylesheets/entity-actions/create/manifests.ts`). The `kind: 'create'` entity action kind is defined in core (`src/Umbraco.Web.UI.Client/src/packages/core/entity-action/common/create/`), which:

1. Discovers all `entityCreateOptionAction` extensions for the entity type
2. If exactly one option exists, delegates directly to it (href or execute)
3. If multiple options exist, opens a list modal (`umb-entity-create-option-action-list-modal`)

This is a sound architectural improvement: each create option is now independently extensible and discoverable, rather than being hard-wired into action classes.

### Severity: Info - Proper Path Pattern Usage

The PR replaces hardcoded URL strings (e.g., `section/settings/workspace/language/create`) with typed `UmbPathPattern` instances (e.g., `UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN`). This improves type safety and maintainability.

New path patterns added in:
- `src/Umbraco.Web.UI.Client/src/packages/members/member-group/paths.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/paths.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/paths.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/templates/paths.ts`

---

## 3. Breaking Changes

### Severity: Medium - Entity Action Alias Renames

Several entity action aliases were changed without deprecation:

| Old Alias | New Alias |
|-----------|-----------|
| `Umb.EntityAction.PartialView.CreateOptions` | `Umb.EntityAction.PartialView.Create` |
| `Umb.EntityAction.Script.CreateOptions` | `Umb.EntityAction.Script.Create` |

External plugin developers who use `umbExtensionsRegistry` to look up, override, or add conditions to these aliases by name will break silently. The old alias simply ceases to exist with no deprecation path.

**Note:** The `Umb.EntityAction.DocumentBlueprint.Create`, `Umb.EntityAction.Language.Create`, `Umb.EntityAction.MemberGroup.Create`, and `Umb.EntityAction.Template.Create` aliases were preserved (same alias, changed `kind` from `default` to `create`). Only partial view and script had the alias rename.

**Recommendation:** Consider registering a deprecated alias that maps to the new one, or at minimum document this as a known breaking change in the release notes.

### Severity: Medium - Deleted Public API Classes Without Deprecation

The following action classes were deleted outright:

- `UmbCreateDocumentBlueprintEntityAction` (`document-blueprints/entity-actions/create/create.action.ts`)
- `UmbLanguageCreateEntityAction` (`language/entity-actions/language-create-entity-action.ts`)
- `UmbCreateMemberGroupEntityAction` (`member-group/entity-actions/create-member-group.action.ts`)
- `UmbPartialViewCreateOptionsEntityAction` (`partial-views/entity-actions/create/create.action.ts`)
- `UmbScriptCreateOptionsEntityAction` (`scripts/entity-actions/create/create.action.ts`)
- `UmbCreateTemplateEntityAction` (`templates/entity-actions/create/create.action.ts`)

These were loaded via dynamic `import()` in manifest registrations and were not re-exported from package `index.ts` files, so the breakage surface is limited. However, any external package that directly imported these files by path would break.

### Severity: Low - Proper Deprecation of Modals

The PR correctly follows the deprecation pattern for modals and their types:

- `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` - marked `@deprecated`, modal registration kept
- `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL` - marked `@deprecated`, modal registration kept
- `UMB_SCRIPT_CREATE_OPTIONS_MODAL` - marked `@deprecated`, modal registration kept

Runtime `UmbDeprecation` warnings are added in `connectedCallback` for the deprecated modal elements. Deprecation messages correctly target "Scheduled for removal in Umbraco 19" (v17 current -> v19 removal = correct per the v17+2 rule).

---

## 4. Code Quality Issues

### Severity: Medium - Document Blueprint Option Action Uses `history.pushState` Instead of `getHref`

In `default-blueprint-create-option-action.ts` (line 41), the action calls `history.pushState(null, '', url)` inside `execute()` instead of returning the URL from `getHref()`. This bypasses the `UmbCreateEntityAction`'s single-option optimization that converts `getHref()` into a native `<a href>` link, which provides:

1. Proper browser link behavior (middle-click to open in new tab, right-click "Copy link address")
2. Better accessibility (screen readers announce it as a link)
3. No JavaScript execution needed

This is done because the blueprint action opens a document type picker modal before navigating, so a static href is not possible. This is functionally correct but worth noting as a design trade-off. The old implementation had the same behavior.

### Severity: Low - Unused `UmbModalToken` Import

In `default-blueprint-create-option-action.ts`, the `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` constant is no longer imported (the old action that used it was deleted). The new action uses `UMB_DOCUMENT_TYPE_PICKER_MODAL` instead, which is a simpler and more direct approach. This is a positive change.

### Severity: Low - Inconsistent `weight` Assignment

The new `entityCreateOptionAction` manifests use `weight: 1000` for default actions and `weight: 900` for the partial view "from snippet" option. Meanwhile, the `entityAction` (kind: `create`) inherits `weight: 1200` from the kind definition. This is consistent with the existing pattern (stylesheets default option has `weight: 100`). However, there is no apparent convention for option action weights. The folder options have no explicit weight. This works because the kind's `create.action.kind.ts` defines default weight only for the parent entity action, not for options.

### Severity: Low - Blueprint Label Not Localized

In `document-blueprints/entity-actions/create/default/manifests.ts` (line 14):
```ts
label: 'Document Blueprint for',
```

This label is a raw English string, not a localization key (which would use the `#section_key` format). Compare with other new manifests that properly use localization keys like `#actions_create`, `#create_newEmptyPartialView`, `#create_folder`, etc. This means the blueprint create option label will not be translatable.

### Severity: Info - Trailing Space Fixed

In `member-group/entity-actions/manifests.ts`, a trailing space was fixed in the delete action name:
```
- name: 'Delete Member Group Entity Action ',
+ name: 'Delete Member Group Entity Action',
```

This is a nice incidental cleanup.

---

## 5. Ellipsis (`...`) UI Changes

### Severity: Low - Inconsistent Ellipsis Application

The PR adds ellipsis (`...`) to labels in multiple places:

1. **Collection create action** (`collection-create-action.element.ts`): Appends `...` when `manifest.meta.additionalOptions` is true
2. **Document create options modal** (`document-create-options-modal.element.ts`): Hardcoded `+ '...'`
3. **Media create options modal** (`media-create-options-modal.element.ts`): Hardcoded `+ '...'`
4. **Member create options modal** (`member-create-options-modal.element.ts`): Hardcoded `+ '...'`

The ellipsis convention indicates that clicking the option will open a further dialog/workflow (standard UX convention). However, the application is inconsistent:

- Some places check `additionalOptions` flag (collection action)
- Others hardcode the ellipsis regardless (document/media/member modals)

This dual approach is because the modals always show options that lead to further interaction, while the collection action conditionally shows it. This is acceptable but could be documented as a convention.

### Severity: Low - Test Locators Updated for Ellipsis

The acceptance test helpers correctly update their locators to match the new `...` suffix:

- `DataTypeUiHelper.ts`: `[name="Data Type"]` -> `[name="Data Type..."]`
- `DocumentTypeUiHelper.ts`: `getByText('Document Type', {exact: true})` -> `getByText('Document Type...', {exact: true})`
- `MediaTypeUiHelper.ts`: `[name="Media Type"]` -> `[name="Media Type..."]`

The `clickCreateActionWithOptionName` in `UiBaseLocators.ts` was updated to search for both variants: `'[label="' + optionName + '"], [label="' + optionName + '..."]'`. This is a pragmatic approach to handle both old and new label formats.

---

## 6. Test Coverage

### Severity: Info - Adequate Test Updates

The acceptance tests are properly updated:

1. **PartialViewUiHelper.ts**: Modal locator updated from `umb-partial-view-create-options-modal` to `umb-entity-create-option-action-list-modal`, and element locators from `uui-menu-item` to `umb-ref-item`
2. **ScriptUiHelper.ts**: Same modal locator change
3. **UiBaseLocators.ts**: `createDocumentBlueprintModal` locator updated to `umb-entity-create-option-action-list-modal`
4. **Spec files**: All calls changed from `clickCreateOptionsActionMenuOption()` to `clickCreateActionMenuOption()`

The `clickCreateOptionsActionMenuOption()` method still exists in `UiBaseLocators.ts` (delegates to `clickEntityActionWithName('CreateOptions')`), which means it would fail if called for partial views or scripts since those aliases no longer exist. The method is not removed, which could cause confusion, but spec files were correctly updated to use `clickCreateActionMenuOption()` instead.

### Severity: Info - Large Formatting Change in UiBaseLocators.ts

Approximately 750 lines of the diff in `UiBaseLocators.ts` are pure formatting changes (single quotes to double quotes, multi-line formatting). This inflates the diff size significantly but is functionally harmless. Ideally this would be a separate commit or PR to keep the refactor diff clean, but this is a minor concern.

---

## 7. Security

No security concerns. The changes are purely frontend extension registration refactoring. No new data flows, no authentication changes, no user input handling changes.

---

## 8. Specific File-Level Notes

### `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/from-snippet/from-snippet-create-option-action.ts`

The "from snippet" option correctly opens the existing `UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL` via `umbOpenModal`. This preserves the existing snippet selection workflow while fitting into the new extensible pattern. Well done.

### `src/Umbraco.Web.UI.Client/src/packages/templating/templates/entity-actions/create/default/manifests.ts`

The template `entityCreateOptionAction` does **not** have the `UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS` condition. This is correct because the production mode condition is on the parent `entityAction` (kind: `create`), which gates the entire "Create" button visibility. The `entityCreateOptionAction` extensions are only discovered when the parent action is active.

### `src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/create/default/default-language-create-option-action.ts`

Cleanly uses `getHref()` instead of `execute()` since it just navigates to a create workspace path. This enables the `UmbCreateEntityAction`'s single-option href optimization.

---

## 9. Verdict

**APPROVE with minor concerns**

This is a well-executed migration that brings six entity types into alignment with the established `entityCreateOptionAction` extension pattern. The deprecation of old modals follows the correct v17+2 rule. The new path patterns replace hardcoded URLs. Test coverage is updated appropriately.

**Key concerns to address before or after merge:**

1. **Entity action alias renames** (Partial View: `CreateOptions` -> `Create`, Script: `CreateOptions` -> `Create`) are breaking changes without a deprecation path. Consider whether external consumers might reference these aliases. If this is deemed acceptable for v17 (since the extension system is still relatively new), it should be documented in release notes.

2. **Blueprint label** `'Document Blueprint for'` should use a localization key instead of a raw English string.

3. **The `clickCreateOptionsActionMenuOption()` method** in `UiBaseLocators.ts` is now orphaned for partial views and scripts. Consider removing it or marking it as deprecated to prevent confusion.

These are minor issues that should not block the merge.
