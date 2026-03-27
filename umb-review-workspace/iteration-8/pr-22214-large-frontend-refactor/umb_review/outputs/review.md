## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25b3897f5e592e11e0b6eaeaa4bdd8` · **Skipped:** 0 files out of 78 total

Migrates create entity actions for Templating (partial views, scripts, templates), Language, and Member Group from bespoke `default`-kind entity actions to the `entityCreateOptionAction` extension pattern, replacing modal-based option selection with composable option actions, and adds collection create actions to tree item children.

- **Modified public API:** Manifest aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` renamed to `Umb.EntityAction.PartialView.Create` and `Umb.EntityAction.Script.Create`. `kind` on Language and Member Group entity actions changed from `default` to `create`. New `UMB_CREATE_PARTIAL_VIEW_WORKSPACE_PATH_PATTERN`, `UMB_CREATE_SCRIPT_WORKSPACE_PATH_PATTERN`, `UMB_CREATE_TEMPLATE_WORKSPACE_PATH_PATTERN`, and `UMB_CREATE_MEMBER_GROUP_WORKSPACE_PATH_PATTERN` path pattern constants added.
- **Affected implementations (outside this PR):** Plugin developers that register conditions or overwrites referencing the old aliases `Umb.EntityAction.PartialView.CreateOptions` or `Umb.EntityAction.Script.CreateOptions` by string will silently break — the runtime will not find those manifest aliases post-merge.
- **Other changes:** The `...` ellipsis suffix is now appended to type-picker items inside Document, Media, and Member create option modals (e.g., document type names rendered as `Blog Post...`). The `folderBtn` locator in test helpers was updated from `uui-menu-item` to `umb-ref-item`, reflecting the new modal element used by the migrated flows.

---

### Critical

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts:7-8`** and **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts:7-8`**: The entity action aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` are renamed to `Umb.EntityAction.PartialView.Create` and `Umb.EntityAction.Script.Create` with no backwards-compatibility entry. Per the manifest/extension system breaking-change rules, alias renames break any plugin that references the old string — compiler won't catch this. The old aliases should be kept as deprecated no-op entries (e.g., an entry with the old alias, same `forEntityTypes`, and a console deprecation note) alongside the new ones, or the old alias should be preserved until removal in v19. The `Umb.EntityAction.Language.Create` and `Umb.EntityAction.MemberGroup.Create` aliases are unchanged (only their `kind` changed), so those are not affected.

---

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:178`**, **`src/Umbraco.Web.UI.Client/src/packages/media/media/entity-actions/create/media-create-options-modal.element.ts:103`**, **`src/Umbraco.Web.UI.Client/src/packages/members/member/entity-actions/create/member-create-options-modal.element.ts:72`**: These modals append a hardcoded `+ '...'` to the rendered item name (e.g., `"Blog Post..."`). This data mutation happens in the template render path, so the displayed value differs from the model. The ellipsis convention should come from the `additionalOptions` flag in the manifest (as done in `collection-create-action.element.ts`) rather than being hardcoded in the item template. If `additionalOptions` is not guaranteed to be set, a guard like `label + (option.additionalOptions ? '...' : '')` is needed; otherwise items without `additionalOptions: true` will incorrectly get ellipses appended. This is also inconsistent with the rest of the PR where the `additionalOptions` flag drives ellipsis display.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:18-27`**: The modal `UMB_DOCUMENT_TYPE_PICKER_MODAL` is opened but the result is accessed as `value.selection[0]` without handling the case where the modal was dismissed (returning `null` or an empty selection gracefully). Unlike `umbOpenModal` throwing on cancel, if it resolves with an empty selection the code reaches `throw new Error('Document type unique is not available')` which is an unhandled error — consumers will see an unformatted error rather than a graceful no-op. Compare this with similar patterns that check for cancellation before accessing the value.

---

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/manifests.ts:16`**: The `label: 'Document Blueprint for'` is a hardcoded English string (not a `#localize_key`), and reads as an incomplete phrase. All other `entityCreateOptionAction` manifests in this PR use localization keys. This was pre-existing (already on `main`), so it's out of strict scope, but the PR touches this file and a follow-up localization key would be consistent.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/default/default-partial-view-create-option-action.ts:10`** and similar (`default-script-create-option-action.ts:10`, `default-template-create-option-action.ts:10`): The `if (!parentEntityType) throw new Error(...)` guard is unreachable — `this.args.entityType` is a string (entity types don't include falsy values and the manifest `forEntityTypes` filter would prevent invocation with an undefined entity type). The guard adds noise; consider removing or converting to an assertion comment.

---

## Request Changes

Critical and important issues must be addressed first.
