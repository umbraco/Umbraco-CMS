## PR Review

**Target:** `origin/main` · **Based on commit:** `d06591cdcdc0a55b8410946ddf568a636989202a` · **Files:** 78 changed, 0 skipped, 78 reviewed

This PR migrates the create entity actions for Templating (partial views, scripts, templates), Language, Member Group, and Document Blueprint from monolithic `entityAction` (kind `default`) implementations to the `entityCreateOptionAction` extension pattern, while deprecating the old custom create modals and internal action classes.

- **Modified public API:** Manifest aliases changed — `Umb.EntityAction.PartialView.CreateOptions` → `Umb.EntityAction.PartialView.Create`; `Umb.EntityAction.Script.CreateOptions` → `Umb.EntityAction.Script.Create`; `Umb.EntityAction.Template.Create` (kind changed from `default` to `create`); `Umb.EntityAction.Language.Create` (kind changed from `default` to `create`); `Umb.EntityAction.MemberGroup.Create` (kind changed from `default` to `create`); `Umb.EntityAction.DocumentBlueprint.Create` (kind changed from `default` to `create`).
- **Other changes:** The old custom create modals (`UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`, `UMB_SCRIPT_CREATE_OPTIONS_MODAL`, `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL`) are deprecated with `@deprecated` JSDoc, runtime `UmbDeprecation.warn()` calls, and kept for backwards compatibility until Umbraco 19. Collection create actions are added for Document Blueprint, Partial View, Script, and Template tree item children. The `...` ellipsis suffix is now conditionally appended to menu item labels when `additionalOptions: true` is set on the manifest meta. E2E tests are updated to use `clickCreateActionMenuOption()` instead of `clickCreateOptionsActionMenuOption()`.

> [!NOTE]
> **Complexity advisory** — This PR may benefit from splitting.
>
> - **Formatting mixed with logic:** `tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts` has 1065 lines changed in full but 1013 lines changed ignoring whitespace. The large majority of the diff is a reformatting of the entire file (single quotes → double quotes, multi-line chaining). One substantive change (`clickCreateActionWithOptionName` matching both `label="X"` and `label="X..."`) is buried in ~1000 lines of formatting noise. Consider a separate formatting-only commit or PR to keep the functional diff reviewable.
>
> _This is an observation, not a blocker. The full review follows below._

---

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/create/default/manifests.ts` (already in main)**: The Default Language create option action has `additionalOptions: true` in its `meta`, which causes the list modal to render the label as "Create..." (with an ellipsis). However, `UmbDefaultLanguageCreateOptionAction.getHref()` returns a direct navigation URL — no additional dialog is opened. The ellipsis implies "opens more options" and is misleading for a terminal-navigation action. Same issue applies to `members/member-group/entity-actions/create/default/manifests.ts` for the member group default option. Compare with `default-blueprint-create-option-action.ts`, where `additionalOptions: true` is correct because it opens the document type picker.

  → Remove `additionalOptions: true` from the `meta` of `Umb.EntityCreateOptionAction.Language.Default` and `Umb.EntityCreateOptionAction.MemberGroup.Default`, since those options navigate directly without presenting further choices.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/manifests.ts`**: The `label` for the default blueprint create option is a raw string `'Document Blueprint for'` instead of a localization key. All other create option action labels in this PR use localization keys (e.g., `#create_newTemplate`, `#create_newEmptyPartialView`, `#actions_create`). Hardcoded English strings break localization for non-English users.

  → Replace `label: 'Document Blueprint for'` with an appropriate localization key. If no key exists yet, one should be added to `en.ts`.

- **`tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`**: The massive reformatting of this file (single quotes → double quotes, multi-line call chains throughout the entire 2000-line file) is bundled with the two functional changes: the `clickCreateActionWithOptionName` selector update and a new `collectionTreeItemTableRow` locator. This makes code review substantially harder and obscures the intent of functional changes in the noise. Per the codebase's clean code principles, it also raises the risk of accidental test breakage if a merge produces conflicts in formatting-only regions.

  → Extract the pure formatting changes into a separate commit (or let a linter/formatter handle it on CI), so the substantive test changes can be reviewed cleanly.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:25`**: After the document type picker modal resolves, the selection is filtered for non-null values with `value.selection.filter((x) => x !== null)`, but then `selection[0]` is accessed without checking if `selection` is empty. If the user somehow gets past the picker with an empty valid selection (e.g., edge case in the modal), the `Error('Document type unique is not available')` will be thrown. This matches the error handling in the deleted `create.action.ts` so it's not a regression, but consider clarifying the guard: `if (!selection.length)` is more idiomatic than `if (!documentTypeUnique)` when the issue is an empty array.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts` and `scripts/entity-actions/create/manifests.ts`**: The deprecated modal registration comment reads `// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.` consistently across document-blueprint and partial-view manifests, which is good. However, the script manifests keep the old `Umb.EntityAction.Script.CreateOptions` alias with `kind: 'default'` in FETCH_HEAD (this may already be resolved in main). Verify the scripts migration applies the same `kind: 'create'` pattern once the PR is rebased.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/options-modal/script-create-options-modal.element.ts`**: The deprecated `umb-script-create-options-modal` element still contains a hardcoded `href` construction: `` `section/settings/workspace/script/create/parent/${...}` `` with a `// TODO: construct url` comment. The new `UmbDefaultScriptCreateOptionAction` correctly uses `UMB_CREATE_SCRIPT_WORKSPACE_PATH_PATTERN.generateAbsolute(...)`. The deprecated modal's hardcoded URL doesn't use the path pattern, so it will not automatically reflect any future path changes. This is acceptable since the modal is deprecated, but the `// TODO` should either be removed (since this code is on the deprecation path) or updated to note it won't be fixed.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions. In particular, the `additionalOptions: true` on the Language and Member Group default create option actions (misleading ellipsis on direct-navigation items) and the hardcoded English label on the Document Blueprint default option are worth addressing before shipping.
