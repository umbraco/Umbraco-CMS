## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25b3897f5e592e11e0b6eaeaa4bdd8` · **Skipped:** 0 noise files out of 78 total

Migrates the create entity actions for Templating (templates, partial views, scripts), Document Blueprints, Language, and Member Group from custom `kind: 'default'` actions opening bespoke option modals to the `kind: 'create'` + `entityCreateOptionAction` extension system. Deprecated modals are preserved in place with `@deprecated` JSDoc and `UmbDeprecation` runtime warnings.

- **Modified public API:** Manifest aliases `Umb.EntityAction.PartialView.CreateOptions` renamed to `Umb.EntityAction.PartialView.Create`; `Umb.EntityAction.Script.CreateOptions` renamed to `Umb.EntityAction.Script.Create`. Modal tokens `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL`, `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`, `UMB_SCRIPT_CREATE_OPTIONS_MODAL`, and their custom element registrations marked `@deprecated` and kept. New exported constants: `UMB_CREATE_MEMBER_GROUP_WORKSPACE_PATH_PATTERN`, `UMB_CREATE_SCRIPT_WORKSPACE_PATH_PATTERN`, `UMB_CREATE_PARTIAL_VIEW_WORKSPACE_PATH_PATTERN`, `UMB_CREATE_TEMPLATE_WORKSPACE_PATH_PATTERN` (new path patterns in `paths.ts` files).
- **Affected implementations (outside this PR):** Plugin developers using `conditions` or `overwrites` targeting the old aliases by string (`Umb.EntityAction.PartialView.CreateOptions`, `Umb.EntityAction.Script.CreateOptions`) will silently break — alias renames are not compiler-caught.
- **Breaking changes:** Alias renames for `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` (see Critical section).
- **Other changes:** `additionalOptions: true` added to several existing `entityCreateOptionAction` manifests (document types, media types, member types, stylesheet), causing `...` to appear in their collection create action labels. Items in document, media, and member create option modals now append `'...'` directly to the rendered name string.

---

### Critical

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/manifests.ts` and `src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/manifests.ts`**: The manifest aliases `Umb.EntityAction.PartialView.CreateOptions` and `Umb.EntityAction.Script.CreateOptions` are renamed to `Umb.EntityAction.PartialView.Create` and `Umb.EntityAction.Script.Create` respectively, with no backward-compat entry preserving the old alias. Per the Manifest/Extension System section of the breaking-changes reference: alias renames silently break any plugin that references the old string in conditions, overwrites, or extension registry lookups. The old aliases should be preserved as deprecated entries that delegate to the new ones, or the renames should not happen within a major version. Fix: keep the old aliases registered (possibly as deprecated no-op overwrites) alongside the new ones until Umbraco 19.

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:178`**, **`src/Umbraco.Web.UI.Client/src/packages/media/media/entity-actions/create/media-create-options-modal.element.ts:103`**, **`src/Umbraco.Web.UI.Client/src/packages/members/member/entity-actions/create/member-create-options-modal.element.ts:72`**: These files append `'...'` unconditionally to the displayed name (`.name=${this.localize.string(documentType.name) + '...'}`). The correct mechanism is `additionalOptions: true` on the manifest driving `collection-create-action.element.ts` to conditionally append `...`. Hardcoding `'...'` in the modal template is redundant if the items are already rendered from manifests with `additionalOptions: true`, and doubly-appends if ever rendered via the collection action. More importantly, it breaks localization — names for some locales may already include punctuation, and `'...'` is not a localized string. The `...` should be a UI concern handled by the rendering component based on `additionalOptions`, not concatenated at the model level.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/default-blueprint-create-option-action.ts:22`**: After `umbOpenModal` resolves, `value.selection` is filtered for `!== null` but then only `selection[0]` is taken and checked for falsiness. If `value.selection` is empty (user cancelled), `documentTypeUnique` will be `undefined` and an `Error` is thrown instead of returning silently. This is inconsistent with how other option actions in the PR handle user cancellation (e.g., `from-snippet-create-option-action.ts` simply returns). An uncaught error from user cancellation will likely produce an error log or UI noise. Prefer: `if (!documentTypeUnique) return;` instead of throwing.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/create/default/manifests.ts:12`** and **`src/Umbraco.Web.UI.Client/src/packages/members/member-group/entity-actions/create/default/manifests.ts:12`**: Both language and member-group `entityCreateOptionAction` manifests set `additionalOptions: true` despite their `execute`/`getHref` implementations performing a single direct navigation (no secondary modal or picker). `additionalOptions: true` signals to the UI that activating this option will open further choices (hence the `...` ellipsis). If there are no additional options, this flag is misleading. Only set `additionalOptions: true` when the action genuinely opens a follow-up modal or picker.

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/default/default-partial-view-create-option-action.ts:8`** and **`src/Umbraco.Web.UI.Client/src/packages/templating/scripts/entity-actions/create/default/default-script-create-option-action.ts:9`** and **`src/Umbraco.Web.UI.Client/src/packages/templating/templates/entity-actions/create/default/default-template-create-option-action.ts:9`**: These all check `if (!parentEntityType) throw new Error(...)` but `parentEntityType` is set from `this.args.entityType as ...`, which can only be falsy if `args.entityType` itself is falsy — something the framework guarantees is always set. The guard is dead code and adds noise. Same observation applies to `default-script-create-option-action.ts:9`.

- **`tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`**: The bulk of this 1065-line change is a quote-style reformatting (single → double quotes) mixed with real locator logic updates from the new modal element tag names. Bundling formatting changes with logic changes makes the functional diff hard to review. Consider separating formatting-only commits in future PRs.

---

## Request Changes

Critical and important issues must be addressed first.
