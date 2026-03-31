# Code Review: PR #22214 -- Migrate Templating, Language, Member Group, and Document Blueprint create entity actions to entityCreateOptionAction extensions

**PR**: #22214
**Branch**: `v17/feature/templating-create-action-options`
**Target**: `origin/main`
**78 files changed** (+1323 / -566)

---

## Summary

This PR migrates several entity "create" actions (Templating: partial views, scripts, templates; Language; Member Group; Document Blueprint) from custom `UmbEntityActionBase` implementations with bespoke modal dialogs to the new `entityCreateOptionAction` extension pattern. This is a continuation of work already done for Stylesheets and Document Types, unifying the create flow architecture across all entity types. Old modals and actions are deprecated (not removed) with deprecation notices targeting Umbraco 19. The PR also adds `collectionAction` manifests for tree-item-children collections, adds ellipsis indicators for items with additional options, reformats the entire `UiBaseLocators.ts` acceptance test helper, and updates all affected E2E tests.

---

## Dimension Ratings

| Dimension | Rating | Notes |
|-----------|--------|-------|
| **Security** | Pass | No security-relevant changes. No credentials, no API surface changes, no auth modifications. |
| **Performance** | Pass | No performance concerns. Lazy imports are maintained. No new hot paths or unbounded operations. |
| **Correctness** | Caution | A few issues identified below. |
| **Maintainability** | Good | The refactoring moves toward a consistent, extensible pattern. Deprecations are properly annotated. |

---

## Findings

### Critical

*None identified.*

### High

#### 1. Entity action alias changes are breaking for external consumers (Correctness)

**Files**:
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts`
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts`

The entity action aliases were renamed:
- `Umb.EntityAction.PartialView.CreateOptions` --> `Umb.EntityAction.PartialView.Create`
- `Umb.EntityAction.Script.CreateOptions` --> `Umb.EntityAction.Script.Create`

Entity action aliases are public extension points. External packages or plugins that override, condition-gate, or reference these aliases by string will silently break. Unlike the modals (which were kept registered with deprecation comments), the old entity action aliases are simply removed with no backwards compatibility shim.

Consider either:
- Keeping the old aliases registered (even as no-ops) with `@deprecated` comments, or
- Documenting the alias changes as intentional breaking changes in the PR description.

For comparison, the `Umb.EntityAction.DocumentBlueprint.Create` and `Umb.EntityAction.Template.Create` aliases were **not** renamed -- they kept their existing alias and only changed from `kind: 'default'` to `kind: 'create'`, which is the correct approach.

#### 2. Missing `weight` on several new entity actions (Correctness)

**Files**:
- `src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/manifests.ts` (line 8)
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts` (line 8)
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts` (line 9)
- `src/Umbraco.Web.UI.Client/src/packages/templating/templates/entity-actions/manifests.ts` (line 8)

The old entity actions had `weight: 1200` to ensure the "Create" action appeared at the top of the actions menu. The new `kind: 'create'` entity actions dropped the `weight` property. The Stylesheet reference implementation (existing pattern this PR follows) does include `weight: 1200`. The Language and Member Group create actions in this same PR correctly include `weight: 1200`. The omission in the four listed files is inconsistent and may cause the Create action to appear in an unexpected position in the actions menu.

### Medium

#### 3. Missing Production Mode condition on script `entityCreateOptionAction` sub-actions (Correctness)

**Observation**: The `Umb.EntityAction.Script.Create` entity action does *not* have the `UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS` condition. Checking the original code, the old script create action also lacked this condition, so this is not a regression introduced by this PR. However, the Partial View and Template entity actions both have this condition. Scripts are also file-system editable resources and the inconsistency is worth noting. This may be intentional (scripts might be needed in production) but could also be an existing oversight.

#### 4. Ellipsis (`...`) appended via string concatenation rather than localization (Maintainability)

**Files**:
- `src/Umbraco.Web.UI.Client/src/packages/core/collection/action/create/collection-create-action.element.ts` (line 10)
- `src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts`
- `src/Umbraco.Web.UI.Client/src/packages/media/media/entity-actions/create/media-create-options-modal.element.ts`
- `src/Umbraco.Web.UI.Client/src/packages/members/member/entity-actions/create/member-create-options-modal.element.ts`

The ellipsis indicator for "additional options" is appended via hard-coded string concatenation (`label + '...'`). This is spread across multiple files. If the UX convention for indicating sub-menus changes (e.g., to a Unicode ellipsis character, or to an icon), every concatenation site must be updated. Consider centralizing this in the `collection-create-action` element logic or in the create option action kind itself.

Additionally, in `collection-create-action.element.ts`, the logic `manifest.meta.additionalOptions ? label + '...' : label` reads `additionalOptions` from the manifest meta at render time. This works but couples the UI rendering to a meta flag that exists on the option actions, not on the collection action itself. If a collection action's options all lack `additionalOptions`, the label will still not show an ellipsis, which seems correct, but the coupling is indirect.

#### 5. `clickCreateActionWithOptionName` uses fragile dual-selector for ellipsis (Maintainability)

**File**: `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts` (around line 2996)

```typescript
const createOptionLocator = this.createActionButtonCollection.locator(
  '[label="' + optionName + '"], [label="' + optionName + '..."]',
);
```

This dual selector works around the fact that some labels now have `...` appended. It means every caller must remember whether to pass the name with or without ellipsis. If the ellipsis convention changes, this selector will break silently. A more robust approach would be to match using a starts-with selector or a regex.

### Low

#### 6. Massive whitespace-only reformatting of UiBaseLocators.ts inflates the diff (Maintainability)

**File**: `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts` (+1065 / -1013 when ignoring whitespace: same diff is reduced significantly)

The bulk of changes in `UiBaseLocators.ts` are reformatting: single quotes to double quotes, adding spaces inside braces, and multi-line formatting. The `stat-ignore-ws.txt` confirms this reduces to just the actual semantic changes. While the reformatting is valid (it likely aligns with a linter configuration), mixing it with functional changes makes the PR significantly harder to review. Consider separating formatting-only changes into a dedicated commit in future PRs.

#### 7. Document Blueprint `default-blueprint-create-option-action.ts` has different modal pattern than the old action (Correctness)

**File**: `src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts`

The old `create.action.ts` used `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` which opened a modal that itself contained a document type picker plus folder creation. The new `default-blueprint-create-option-action.ts` directly opens `UMB_DOCUMENT_TYPE_PICKER_MODAL` instead. This is a functional change -- the old modal's folder creation capability is now handled by the separate `folder/manifests.ts` entity create option action, which is the correct decomposition. Just noting this is intentional restructuring, not a 1:1 port.

#### 8. Trailing whitespace in member group entity action name (Style)

**File**: `src/Umbraco.Web.UI.Client/src/packages/members/member-group/entity-actions/manifests.ts`

The old code had `name: 'Delete Member Group Entity Action '` (trailing space). The new code correctly trims it to `name: 'Delete Member Group Entity Action'`. This is a positive fix.

---

## Positive Observations

1. **Consistent deprecation strategy**: Old modals are kept registered with clear `@deprecated` JSDoc comments and runtime `UmbDeprecation` warnings targeting Umbraco 19. This gives external consumers time to migrate.

2. **Follows established pattern**: The refactoring closely follows the Stylesheet create action pattern (already merged), which uses `kind: 'create'` entity actions with `entityCreateOptionAction` sub-extensions. This consistency makes the codebase more predictable.

3. **Proper path pattern usage**: New code uses `UmbPathPattern` for URL generation instead of hard-coded URL strings (replacing several `// TODO` comments about avoiding hardcoded URLs). This is a clear improvement.

4. **E2E tests updated in lockstep**: All affected acceptance test helpers and specs are updated to match the new UI structure (new modal element names, new locators, new method names). This demonstrates thorough testing discipline.

5. **Clean decomposition**: The old monolithic create actions (which opened a single modal containing all options) are properly decomposed into individual `entityCreateOptionAction` extensions, each responsible for one creation path. This enables external packages to add their own create options without forking the modal.

---

## Summary of Action Items

| Priority | Item | Action |
|----------|------|--------|
| High | Alias changes for PartialView.CreateOptions and Script.CreateOptions | Add backwards-compatible alias registrations or document as intentional breaking change |
| High | Missing `weight` on 4 entity actions | Add `weight: 1200` to match existing patterns (Stylesheet, Language, MemberGroup) |
| Medium | Hard-coded ellipsis concatenation | Consider centralizing the "..." suffix logic |
| Low | Large formatting diff in UiBaseLocators.ts | Split formatting-only changes into separate commit (future guidance) |
