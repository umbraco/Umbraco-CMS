## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25` · **Files:** 78 changed, 0 skipped, 78 reviewed (0 full, 78 diff + header-only)

Migrates create entity actions for Templating (Partial Views, Scripts, Templates), Language, Member Group, and Document Blueprint from custom `entityAction` (kind: `default`) implementations to the `entityCreateOptionAction` extension system. Deprecated modals and interfaces are preserved with `@deprecated` JSDoc and runtime `UmbDeprecation` warnings for backwards compatibility.

- **Modified public API:** Entity action aliases renamed: `Umb.EntityAction.PartialView.CreateOptions` to `Umb.EntityAction.PartialView.Create`, `Umb.EntityAction.Script.CreateOptions` to `Umb.EntityAction.Script.Create`. Entity action kind changed from `default` to `create` for Document Blueprint, Language, Member Group, Template, Partial View, and Script create actions.
- **Breaking changes:** The extension manifest aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` are renamed without preserving the old alias. Plugin developers who use conditions, overwrites, or filtering against these aliases will break silently at runtime (the old alias simply no longer exists). See details in the Critical section below.
- **Other changes:** Ellipsis (`...`) is appended to labels in create option modals for documents, media, and members when `additionalOptions: true` is set on the manifest meta. The `additionalOptions: true` flag was added to data-type, document-type, media-type, member-type, and stylesheet create option manifests. New `UMB_CREATE_*_WORKSPACE_PATH_PATTERN` constants are exported from paths.ts files for partial views, scripts, templates, and member groups. Tree item children collections for document blueprints, partial views, scripts, and templates now register `collectionAction` (kind: `create`) manifests.

> [!NOTE]
> **Complexity advisory** -- This PR may benefit from splitting.
>
> - **Size:** 78 reviewable files with 1323 insertions and 566 deletions spanning 2 projects (`src/Umbraco.Web.UI.Client` and `tests/Umbraco.Tests.AcceptanceTest`). The frontend code changes and acceptance test locator reformatting are independently functional.
> - **Formatting mixed with logic:** `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts` contains significant formatting changes (quote style normalization, line-break reformatting) mixed with functional locator updates. A separate formatting-only commit would keep the functional diff reviewable.
>
> _This is an observation, not a blocker. The full review follows below._

---

### Critical

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts:8`**: The entity action alias was renamed from `Umb.EntityAction.PartialView.CreateOptions` to `Umb.EntityAction.PartialView.Create`. Plugin developers who reference the old alias (e.g., via `overwrites`, conditions like `{ alias: 'Umb.EntityAction.PartialView.CreateOptions' }`, or manifest manipulation) will silently break. The old modal aliases are correctly preserved for backwards compatibility, but the entity action alias is not. -> Register a second manifest entry with the old alias `Umb.EntityAction.PartialView.CreateOptions` (kind `create`, same forEntityTypes and conditions) marked with a `// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.` comment, or keep the old alias on the main manifest and introduce the new one as an additional registration.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts:8`**: Same issue: `Umb.EntityAction.Script.CreateOptions` renamed to `Umb.EntityAction.Script.Create` without preserving the old alias. -> Apply the same fix as for partial views above.

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:14`**: The new `UmbDefaultBlueprintCreateOptionAction` class uses the `execute()` override to open a document-type picker modal, then uses `history.pushState` directly for navigation. While the old `create.action.ts` also used `history.pushState`, the new implementation loses the `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` flow entirely (the old modal showed allowed document types based on the parent). The new implementation uses `UMB_DOCUMENT_TYPE_PICKER_MODAL` directly without the parent context. Verify this preserves the intended UX: will users still see only the document types allowed for the blueprint location, or will they see all document types?

- **`src/Umbraco.Web.UI.Client/src/packages/core/collection/action/create/collection-create-action.element.ts:145`**: The ellipsis is appended via `manifest.meta.additionalOptions ? label + '...' : label`. This accesses `manifest.meta.additionalOptions` which may not be typed on the `ManifestEntityCreateOptionAction` interface meta. If TypeScript does not flag this, it likely works due to loose typing on the meta object, but it would be better to ensure `additionalOptions` is part of the manifest meta type definition. -> Verify that `ManifestEntityCreateOptionAction`'s meta interface includes `additionalOptions?: boolean`, or add it.

- **`tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`**: This file has extensive formatting changes (single-quote to double-quote, brace spacing, line wrapping) mixed with functional changes (locator updates for the new modal elements like `umb-entity-create-option-action-list-modal` replacing `umb-document-blueprint-options-create-modal` and `umb-script-create-options-modal`). This makes it difficult to verify the functional correctness of the locator changes. The functional changes include updating `createDocumentBlueprintModal` to target `umb-entity-create-option-action-list-modal` and updating the `clickCreateActionWithOptionName` method to accept labels with or without `...` suffix. -> Consider separating formatting changes from functional locator updates to ease review.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:178`**: The ellipsis is hardcoded as `+ '...'` directly in the HTML template. Same pattern in `media-create-options-modal.element.ts:103` and `member-create-options-modal.element.ts:72`. Since the `collection-create-action.element.ts` uses `manifest.meta.additionalOptions` to conditionally add the ellipsis, it would be more consistent to use a shared approach. As-is, these modals always append the ellipsis to every type name. If this is the intended design (all types in these modals always lead to additional configuration steps), this is fine but worth a brief comment.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/from-snippet/from-snippet-create-option-action.ts:7`**: The `execute()` override opens a modal but does not return any href. The base class `UmbEntityCreateOptionActionBase` likely has both `getHref()` and `execute()` as overridable methods, and other actions like `UmbDefaultPartialViewCreateOptionAction` use `getHref()`. This is fine as an API design (modal-based actions use `execute()`, navigation-based use `getHref()`), just noting the dual-pattern for awareness.

- **`src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/create/default/default-language-create-option-action.ts:6`**: The `getHref()` method calls `UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN.generateAbsolute({})` with an empty object. The path pattern might expect parameters based on its definition. -> Verify the path pattern truly requires no parameters, or if `{}` is correct for a root-level entity create path.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/manifests.ts:16`**: The deprecated modal manifest comment says `// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.` which is consistent with version.json (major 17, removal in 19). Good.

- **`tests/Umbraco.Tests.AcceptanceTest/lib/helpers/DataTypeUiHelper.ts:315`**: The locator changed from `[name="Data Type"]` to `[name="Data Type..."]` to match the new ellipsis label. This couples the test directly to the display label format. If the ellipsis logic changes, these locators break. Consider using a more resilient selector strategy (e.g., attribute matching with a `*=` operator or testid-based locators).

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions. The entity action alias renames for Partial View and Script are flagged as critical because they are breaking changes for plugin developers who reference these aliases. The old aliases should be preserved alongside the new ones for backwards compatibility, following the same pattern used for the deprecated modal manifests.
