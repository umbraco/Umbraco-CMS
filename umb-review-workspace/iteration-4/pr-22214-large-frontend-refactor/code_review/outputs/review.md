### Code review

Found 2 issues:

**1. Entity action alias renaming is a breaking change without deprecation shim** (CLAUDE.md compliance: breaking changes)

The aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` are renamed to `Umb.EntityAction.PartialView.Create` and `Umb.EntityAction.Script.Create` respectively. External consumers who reference these aliases in conditions, overrides, or extension manifests will silently break. Unlike the modal tokens and elements which are properly deprecated with `@deprecated` JSDoc and `UmbDeprecation` runtime warnings, the old entity action aliases are simply removed with no backwards compatibility path. The PR should either keep the old aliases as deprecated shims or document this as a known breaking change.

Files:
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts` (alias change from `Umb.EntityAction.PartialView.CreateOptions` to `Umb.EntityAction.PartialView.Create`)
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts` (alias change from `Umb.EntityAction.Script.CreateOptions` to `Umb.EntityAction.Script.Create`)

Confidence: 85

**2. `additionalOptions: true` set on folder create option actions where it does not apply** (bug / UX inconsistency)

The `additionalOptions: true` flag is set on all folder `entityCreateOptionAction` manifests (Document Blueprint Folder, Partial View Folder, Script Folder). This flag controls whether an ellipsis ("...") indicator is appended to the label in the collection create action popover (see `collection-create-action.element.ts` line 145: `label + '...'`), implying that clicking the item leads to further sub-options. However, folder creation is a direct action (it prompts for a folder name inline) with no sub-options. Setting `additionalOptions: true` on folder actions results in misleading UX where the folder option shows as "Folder..." suggesting there are more choices when there are none.

Files:
- `src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/folder/manifests.ts:18`
- `src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/folder/manifests.ts:16`
- `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/folder/manifests.ts:16`

Confidence: 80
