# Code Review: PR #22214 - Migrate Templating, Language, Member Group, and Document Blueprint Create Entity Actions

## PR Summary

This PR migrates several entity "create" actions from custom modal-based implementations to the standardized `entityCreateOptionAction` extension system. The affected areas are:

- **Templating**: Partial Views, Scripts, Templates
- **Language**
- **Member Group**
- **Document Blueprints**

Additionally, it adds `additionalOptions: true` to several existing `entityCreateOptionAction` manifests (Data Types, Document Types, Media Types, Member Types, Stylesheets) and appends ellipsis (`...`) to labels in various create modals to indicate that further user input is needed.

**78 files changed**, +1323 / -566 lines (though a large portion is reformatting of `UiBaseLocators.ts`).

---

## Architecture Assessment

### Pattern Consistency: GOOD

The PR correctly follows the established `entityCreateOptionAction` pattern already used by Document Types, Media Types, and Stylesheets. Each migrated entity type now has:

1. An `entityAction` with `kind: 'create'` (which uses `UmbCreateEntityAction` from the core)
2. One or more `entityCreateOptionAction` extensions that provide the actual create options
3. Deprecated modals kept for backwards compatibility with `@deprecated` JSDoc and `UmbDeprecation` runtime warnings

The `kind: 'create'` entity action kind (defined in `core/entity-action/common/create/create.action.kind.ts`) provides sensible defaults: `weight: 1200`, `icon: 'icon-add'`, `label: '#actions_createFor'`, and `additionalOptions: true`. This means the individual manifests don't need to repeat these.

### Backwards Compatibility: GOOD

Deprecated modals are preserved with clear deprecation notices:
- `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL`
- `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`
- `UMB_SCRIPT_CREATE_OPTIONS_MODAL`

Each deprecated item includes:
- `@deprecated` JSDoc annotations with "Scheduled for removal in Umbraco 19"
- Runtime `UmbDeprecation` warnings in `connectedCallback()`
- Modal manifests kept registered for external consumers

---

## Issues Found

### 1. BREAKING CHANGE - Entity Action Alias Renamed (Severity: Medium)

**Files:**
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts`

**Before:**
```
alias: 'Umb.EntityAction.PartialView.CreateOptions'
alias: 'Umb.EntityAction.Script.CreateOptions'
```

**After:**
```
alias: 'Umb.EntityAction.PartialView.Create'
alias: 'Umb.EntityAction.Script.Create'
```

The entity action aliases have been changed. External packages or plugins that override, extend, or depend on these specific aliases (e.g., through conditions, extension registry lookups, or manifest overrides) will break silently. The old alias-based action class files (`create.action.ts`) are deleted entirely with no backwards-compatible fallback.

For consistency with the project's deprecation policy, the old aliases should either be kept as deprecated aliases or the old action classes should remain registered as deprecated extensions. The Document Blueprint entity action alias (`Umb.EntityAction.DocumentBlueprint.Create`) was NOT renamed -- it was already using the `Create` suffix on the previous implementation, so it's consistent.

**Recommendation:** Consider keeping the old aliases registered as deprecated extensions (similar to how the modals are kept), or at minimum document this as a known breaking change.

### 2. Missing Production Mode Condition on entityCreateOptionAction Manifests (Severity: Low)

**Files:**
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/default/manifests.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/from-snippet/manifests.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/folder/manifests.ts`

The Partial View `entityAction` with `kind: 'create'` correctly has the `UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS` condition (match: false), preventing creation in production mode. However, the individual `entityCreateOptionAction` extensions for partial views do NOT have this condition.

This is likely fine in practice because the `UmbCreateEntityAction` (the `kind: 'create'` action) gates access to the option actions. But if another extension or code path directly queries the `entityCreateOptionAction` registry for partial view entity types, the option actions would appear even in production mode.

The old partial view entity actions manifest on main branch (before this PR's branch diverged) had this same pattern, so it appears this was an intentional design choice. The `entityAction` acts as the gatekeeper.

**Not a bug**, but worth noting for awareness.

### 3. Inconsistency: Script Create Action Missing Production Mode Condition (Severity: Low)

**File:** `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts`

The old Script entity action (`kind: 'default'`, alias `Umb.EntityAction.Script.CreateOptions`) did NOT have the `UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS` condition, and the new one doesn't either. Meanwhile, the Partial View and Template entity actions DO have it. This inconsistency predates this PR but is worth flagging.

**Pre-existing issue**, not introduced by this PR.

### 4. Ellipsis Appended to Labels Without Localization Awareness (Severity: Low)

**Files:**
- `src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts`
- `src/Umbraco.Web.UI.Client/src/packages/media/media/entity-actions/create/media-create-options-modal.element.ts`
- `src/Umbraco.Web.UI.Client/src/packages/members/member/entity-actions/create/member-create-options-modal.element.ts`

**Code pattern:**
```typescript
.name=${this.localize.string(documentType.name) + '...'}
```

The ellipsis `...` is hardcoded as a literal string concatenated after a localized string. In some languages/cultures, the ellipsis character or its placement may differ. The `additionalOptions` approach used in the `entityCreateOptionAction` modal rendering (`label + '...'`) has the same issue.

This is a minor i18n concern. A more robust approach might use the Unicode ellipsis character (`\u2026`) or a localized suffix. However, this is consistent with the existing pattern in the `collection-create-action.element.ts` and `entity-create-option-action-list-modal.element.ts`, so it follows convention.

### 5. Massive Reformatting of UiBaseLocators.ts (Severity: Low - Review Concern)

**File:** `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`

This file has 1065 lines changed but the vast majority is reformatting: changing single quotes to double quotes, reformatting multi-line expressions, and adjusting spacing. The actual semantic changes are minimal:

- `createDocumentBlueprintModal` locator changed from `umb-document-blueprint-options-create-modal` to `umb-entity-create-option-action-list-modal`
- `createNewDocumentBlueprintBtn` locator updated similarly
- `scriptCreateModal` locator changed from `umb-script-create-options-modal` to `umb-entity-create-option-action-list-modal`
- `partialViewCreateModal` locator changed similarly
- Selector updates for names with `...` appended (e.g., `[name="Data Type..."]`)

The reformatting makes reviewing the actual changes in this file very difficult. It would be better practice to separate formatting changes into a dedicated commit.

### 6. Deleted Action Classes Not Deprecated First (Severity: Medium)

**Files deleted:**
- `src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/create.action.ts` (`UmbCreateDocumentBlueprintEntityAction`)
- `src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/language-create-entity-action.ts` (`UmbLanguageCreateEntityAction`)
- `src/Umbraco.Web.UI.Client/src/packages/members/member-group/entity-actions/create-member-group.action.ts` (`UmbCreateMemberGroupEntityAction`)
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/create.action.ts` (`UmbPartialViewCreateOptionsEntityAction`)
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/create.action.ts` (`UmbScriptCreateOptionsEntityAction`)
- `src/Umbraco.Web.UI.Client/src/packages/templating/templates/entity-actions/create/create.action.ts` (`UmbCreateTemplateEntityAction`)

These exported classes were available for external consumers to import and extend. While they are TypeScript/JavaScript classes (not .NET public APIs), plugin developers who imported these classes directly would experience a breaking change. The modals were properly deprecated but the action classes were not.

For consistency with the deprecation approach used for the modals, these classes could have been kept with `@deprecated` annotations and `UmbDeprecation` runtime warnings.

### 7. Test Locator Fragility with Ellipsis (Severity: Low)

**Files:**
- `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/DataTypeUiHelper.ts`
- `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/DocumentTypeUiHelper.ts`
- `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/MediaTypeUiHelper.ts`

Locators are updated to match the new `...` suffix:
```typescript
this.dataTypeBtn = this.createOptionActionListModal.locator('[name="Data Type..."]');
this.mediaTypeBtn = this.createOptionActionListModal.locator('[name="Media Type..."]');
this.createDocumentTypeBtn = this.createDocumentModal.locator('umb-ref-item').getByText('Document Type...', {exact: true});
```

The `clickCreateActionWithOptionName` method in `UiBaseLocators.ts` now handles both with and without ellipsis:
```typescript
const createOptionLocator = this.createActionButtonCollection.locator(
  '[label="' + optionName + '"], [label="' + optionName + '..."]',
);
```

This dual selector approach is reasonable but introduces a pattern where callers need to be aware of the ellipsis behavior. It could lead to confusion if a test passes the name with `...` already appended.

---

## Code Quality Observations

### Positive

1. **Consistent pattern application**: All migrated entities follow the same structure (entity action with `kind: 'create'`, separate `entityCreateOptionAction` manifests, path patterns for URL generation).

2. **Proper use of `UmbPathPattern`**: New path patterns are created (e.g., `UMB_CREATE_PARTIAL_VIEW_WORKSPACE_PATH_PATTERN`, `UMB_CREATE_SCRIPT_WORKSPACE_PATH_PATTERN`) instead of hardcoded URL strings. This eliminates TODO comments like "// TODO: Lets avoid having such hardcoded URLs".

3. **Good deprecation documentation**: The deprecated modals have `@deprecated` JSDoc, runtime `UmbDeprecation` warnings, and clear "Scheduled for removal in Umbraco 19" messaging.

4. **API export convention**: New action classes consistently use `export { ClassName as api }` for lazy-loaded extension API resolution.

5. **Weight values**: The `entityCreateOptionAction` manifests use `weight: 1000` for defaults and `weight: 900` for secondary options (e.g., "from snippet"), establishing a clear ordering.

### Areas for Improvement

1. **Document Blueprint label not localized**: The label `'Document Blueprint for'` in `default-blueprint-create-option-action.ts` manifest is a hardcoded English string, not a localization key (should be prefixed with `#` like `'#create_documentBlueprintFor'`).

2. **Redundant `additionalOptions: true`**: The `kind: 'create'` entity action kind already sets `additionalOptions: true` by default. Yet some `entityCreateOptionAction` manifests also set `additionalOptions: true`. While not harmful (it's used on the option level to append `...` to labels), the naming overlap between entity action meta and option action meta could be confusing.

---

## Summary

| Category | Count |
|----------|-------|
| Breaking Changes (alias renames, deleted classes) | 2 |
| Potential Issues | 2 |
| Style/Consistency | 3 |
| Positive Patterns | 5 |

**Overall Assessment**: The PR achieves its goal of standardizing entity create actions onto the `entityCreateOptionAction` extension system. The architectural approach is sound and follows established patterns. The main concerns are: (1) entity action aliases being renamed without deprecation, and (2) action classes being deleted rather than deprecated. These could impact external plugin developers. The large-scale reformatting of `UiBaseLocators.ts` obscures the actual test changes and should ideally be a separate commit.

**Recommendation**: Approve with minor changes requested around alias backwards compatibility and class deprecation strategy.
