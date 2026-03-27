## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25` · **Files:** 78 changed, 0 skipped, 78 reviewed

Migrates Script, Template, Partial View, Document Blueprint, Language, and Member Group create entity actions from the ad-hoc `kind: 'default'` pattern to the standardised `kind: 'create'` pattern with `entityCreateOptionAction` extensions, preserving old modal tokens/elements as deprecated-but-functional for backwards compatibility.

- **Modified public API:** `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL`, `UmbDocumentBlueprintOptionsCreateModalData`, `UmbDocumentBlueprintOptionsCreateModalValue` (deprecated in place); `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`, `UmbPartialViewCreateOptionsModalData` (deprecated in place); `UMB_SCRIPT_CREATE_OPTIONS_MODAL`, `UmbScriptCreateOptionsModalData` (deprecated in place); manifests arrays retyped from `Array<UmbExtensionManifest>` to `Array<UmbExtensionManifest | UmbExtensionManifestKind>` in multiple barrel files
- **Breaking changes:** None — deleted action classes (`UmbCreateDocumentBlueprintEntityAction`, `UmbLanguageCreateEntityAction`, `UmbCreateMemberGroupEntityAction`, `UmbPartialViewCreateOptionsEntityAction`, `UmbScriptCreateOptionsEntityAction`) were internal implementation details, not reachable via any `package.json` `exports` path
- **Other changes:** Document type, media type, and member type names in the remaining legacy create-options modals (Documents, Media, Members) now display a trailing `…` (hardcoded string concatenation). New `UMB_CREATE_*_WORKSPACE_PATH_PATTERN` constants added to `paths.ts` files for partial-views, scripts, templates, and member-group — these are exported via their respective barrel files and therefore become new public API additions.

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **Size:** 1,889 lines changed across 78 files spanning two projects (`Umbraco.Web.UI.Client` and `Umbraco.Tests.AcceptanceTest`). If the `UiBaseLocators.ts` formatting churn and the test helper updates were separated into a prior cleanup commit (or PR), the functional diff would be considerably smaller and easier to review in isolation.
>
> _This is an observation, not a blocker. The full review follows below._

---

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:178`**: The item name is changed from `this.localize.string(documentType.name)` to `this.localize.string(documentType.name) + '...'` (hardcoded string concatenation). In this modal, clicking a document type navigates directly to the creation workspace — there is no further "options" step. Appending `…` implies additional interaction to follow, which may mislead users who see "Article…" and expect a sub-dialog. The same concern applies to `media-create-options-modal.element.ts:103` and `member-create-options-modal.element.ts:72`. If the semantic intent is "this option leads to a workspace with further fields", the `…` may be acceptable, but it should be driven by `additionalOptions` metadata rather than hardcoded, so the behaviour stays consistent with the rest of the create action system. Consider replacing the hardcoded concatenation with a manifest-driven flag, or document why hardcoding is intentional for these three legacy modals.

- **`tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`**: The file contains 1,065 lines of diff but only ~52 lines of functional change. The rest is a wholesale reformatting from single-quotes to double-quotes and single-line constructor chains to multi-line chains. Formatting changes mixed into a functional PR create noise for reviewers and inflate the diff. The skip logic in `clickCreateActionWithOptionName` (now matching both `[label="X"]` and `[label="X..."]`) is the only meaningful logic change in this file and is obscured by the surrounding noise. The formatting change is consistent with a style guide (if the project uses double-quote style), but should ideally be a separate, dedicated commit.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/members/member-group/paths.ts:139`**: `UMB_CREATE_MEMBER_GROUP_WORKSPACE_PATH_PATTERN` is created without a generic type parameter (`new UmbPathPattern('create', ...)`) whereas the analogous patterns for partial-views, scripts, and templates all include explicit generics. Since member-group create takes no URL segments this is functionally correct, but adding `<Record<string, never>>` or a named empty type would make the pattern self-documenting and consistent with the other path patterns in this PR.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:27`**: `const selection = value.selection.filter((x) => x !== null)` followed immediately by `const documentTypeUnique = selection[0]` — if the picker was opened but the user dismissed without selecting, `value` would be the resolved value of a cancelled modal, which typically throws or is an empty object. Depending on `umbOpenModal` semantics the filter+index access could still yield `undefined`. A more explicit guard (`if (!selection.length)`) or relying on the existing `if (!documentTypeUnique)` check is fine, but the intent of the `filter` then `[0]` without length check deserves a comment or a combined expression for clarity.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/options-modal/partial-view-create-options-modal.element.ts`** and **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/options-modal/script-create-options-modal.element.ts`**: The `UmbDeprecation` runtime warning is added, which is good. However the `document-blueprint-options-create-modal.element.ts` deprecation message says "A parent is required to create a folder" in the existing error check — this error message mentions "folder" when the element is actually for document blueprints. This is a pre-existing issue, not introduced by this PR, but since the file is being touched it would be a good opportunity to correct it: `throw new Error('A parent is required to create a document blueprint')`.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.
