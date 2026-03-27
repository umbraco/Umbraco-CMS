## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25b3897f5e592e11e0b6eaeaa4bdd8` · **Files:** 78 changed, 0 skipped, 78 reviewed

This PR migrates the create entity actions for Templating (partial views, scripts, templates), Language, Member Group, and Document Blueprint from ad-hoc `entityAction`+`kind: 'default'` patterns to the `entityCreateOptionAction` extension system, enabling a unified create flow with extensible options. It also adds `collectionAction` create buttons for tree item children collections and updates acceptance tests to match the new UI element selectors.

- **Modified public API:** Manifest aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` renamed; `UmbDocumentBlueprintOptionsCreateModalData`, `UmbDocumentBlueprintOptionsCreateModalValue`, `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL`, `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`, `UmbScriptCreateOptionsModalData`, `UMB_SCRIPT_CREATE_OPTIONS_MODAL`, and their modal element custom elements marked `@deprecated`
- **Other changes:** Create option action items in the "Create" popup now show a trailing `...` when `additionalOptions: true` is set on the manifest meta. The hardcoded `'...'` suffix on document type names in `document-create-options-modal.element.ts` and media/member create modals is now consistently appended regardless of any `additionalOptions` flag.

---

### Critical

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts`**: The manifest alias `Umb.EntityAction.PartialView.CreateOptions` is renamed to `Umb.EntityAction.PartialView.Create`. Plugin developers may reference this alias as a string in manifest `conditions`, `overwrites`, or extension registry lookups. This is a breaking change per the manifest alias rename rule — the old alias silently stops matching without a compile-time error. The old alias should be preserved as a deprecated entry until Umbraco 19. Add a second registration keeping `Umb.EntityAction.PartialView.CreateOptions` as a deprecated alias pointing to the same `kind: 'create'` action (or mark it disabled), so existing overwrites continue to resolve.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts`**: Same alias rename from `Umb.EntityAction.Script.CreateOptions` to `Umb.EntityAction.Script.Create`. Plugin developers who registered conditions or overwrites against `Umb.EntityAction.Script.CreateOptions` will silently lose those registrations. The old alias must be preserved for at least the v18 lifecycle.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/folder/manifests.ts:18`**, **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/folder/manifests.ts:16`**, **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/folder/manifests.ts:16`**: The `additionalOptions: true` flag on folder create option actions appends `...` to the "Folder" label in the UI, implying a sub-selection step. Folders are created directly with a single name input — there is no further option selection. Consider removing `additionalOptions: true` from these folder manifests to avoid misleading UX.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:177`**, **`src/Umbraco.Web.UI.Client/src/packages/media/media/entity-actions/create/media-create-options-modal.element.ts:102`**, **`src/Umbraco.Web.UI.Client/src/packages/members/member/entity-actions/create/member-create-options-modal.element.ts:67`**: These modal elements unconditionally append `'...'` to item names (e.g. `.name=${this.localize.string(documentType.name) + '...'}`). This is a pre-existing pattern being extended consistently, which is fine, but it is not driven by the `additionalOptions` flag from the corresponding manifest meta. If a document type is ever listed here without additional options, the ellipsis will be misleading. A comment noting this intentional choice (or tying it to the `additionalOptions` property) would aid future maintainers.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/default/default-partial-view-create-option-action.ts:11`** and **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/default/default-script-create-option-action.ts:8`**: The guard `if (!parentEntityType) throw new Error(...)` will never trigger because `this.args.entityType` is typed as `string` and a cast to a narrower type cannot produce a falsy value. The cast on the line before (`as UmbPartialViewRootEntityType | UmbPartialViewFolderEntityType`) does not perform any runtime check. The guard is harmless but may give false assurance. Same pattern in `default-script-create-option-action.ts:8`.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions. The two alias renames (`Umb.EntityAction.PartialView.CreateOptions` → `Umb.EntityAction.Script.CreateOptions` → new names) are the most consequential change: plugin developers using overwrites or conditions against the old aliases will silently break without a compiler error. If those aliases were only ever used internally, this is a non-issue — but it warrants explicit confirmation before finalising.
