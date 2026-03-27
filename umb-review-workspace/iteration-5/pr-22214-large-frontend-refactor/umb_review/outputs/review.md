## PR Review

**Target:** `origin/main` · **Based on commit:** `6c40f4aefa25b3897f5e592e11e0b6eaeaa4bdd8` · **Files:** 78 changed, 0 skipped, 78 reviewed

Migrates the "create" entity actions for Templating (Partial Views, Scripts, Templates), Language, Member Group, and Document Blueprints from ad-hoc `UmbEntityActionBase` subclasses to the `entityCreateOptionAction` extension pattern, and adds `additionalOptions: true` flags (with matching `...` ellipsis in the UI) to several existing create option manifests.

- **Modified public API:** Manifest alias `Umb.EntityAction.PartialView.CreateOptions` renamed to `Umb.EntityAction.PartialView.Create`; `Umb.EntityAction.Script.CreateOptions` renamed to `Umb.EntityAction.Script.Create`. Modal token `UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL`, `UMB_SCRIPT_CREATE_OPTIONS_MODAL`, and `UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL` deprecated in place.
- **Other changes:** Items in the document-create, media-create, and member-create option modals now always render a trailing `...` in their name. Collection create actions are now wired up for document-blueprint, partial-view, script, and template tree item children collections.

---

### Important

- **`src/Umbraco.Web.UI.Client/src/packages/documents/documents/entity-actions/create/document-create-options-modal.element.ts:178`**: The ellipsis is appended unconditionally (`this.localize.string(documentType.name) + '...'`), regardless of whether the document type actually opens a further dialog. The same hardcoding is present in `media/media/entity-actions/create/media-create-options-modal.element.ts:101` and `members/member/entity-actions/create/member-create-options-modal.element.ts:70`. By contrast, `collection-create-action.element.ts` correctly conditions the ellipsis on `manifest.meta.additionalOptions`. The hardcoded approach means future option types that do NOT require additional input will still show `...`, misleading users. Consider reading `additionalOptions` from the option/document-type metadata rather than appending unconditionally, or — if all items in these modals are guaranteed to need further input — add a code comment explaining that invariant so the inconsistency is intentional and documented.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/default/manifests.ts:13`**: The label for the "default" document-blueprint create option is `'Document Blueprint for'` — a sentence fragment ending with "for" (implying something follows). This will render in the UI as-is and looks incomplete. Compare with similar actions that use `'#actions_create'` or a proper label like `'New Document Blueprint'`. This should be a localisation key or a complete phrase.

- **`src/Umbraco.Web.UI.Client/src/packages/documents/document-blueprints/entity-actions/create/folder/manifests.ts:13`** (and equivalent in partial-views, scripts): `additionalOptions: true` is set on the folder `entityCreateOptionAction`. The `kind: 'folder'` already opens a "create folder" prompt, so the `additionalOptions` flag is semantically correct. However, this also means the collection-level `collectionAction` item (which delegates to the same entity action) will append `...` in the list modal. Verify this is the intended UX, since the folder create flow is initiated from the tree action list, and the double `...` path (collection action → entity action → folder prompt) may be confusing.

### Suggestions

- **`src/Umbraco.Web.UI.Client/src/packages/templating/partial-views/entity-actions/create/default/default-partial-view-create-option-action.ts:9`**: The guard `if (!parentEntityType) throw new Error(...)` will never trigger because `this.args.entityType` is typed as a non-nullable string in `UmbEntityCreateOptionActionArgs`. The same pattern appears in `default-script-create-option-action.ts:9` and `default-template-create-option-action.ts:9`. While defensive, the comment in the original code may be worth preserving as a `// should never happen` note, but the check itself is dead code. Consider removing it to avoid confusion.

- **`tests/Umbraco.Tests.AcceptanceTest/lib/helpers/UiBaseLocators.ts`**: This file has a large formatting-only change (single-quotes → double-quotes, trailing semicolons, indentation). The functional locator changes (`clickCreateOptionsActionMenuOption` kept, `clickCreateActionMenuOption` added) are buried in 1,000+ lines of reformatting. Consider submitting the formatting change as a separate commit to keep functional changes reviewable in isolation.

- **`src/Umbraco.Web.UI.Client/src/packages/members/member-group/entity-actions/create/default/manifests.ts`** and **`src/Umbraco.Web.UI.Client/src/packages/language/entity-actions/create/default/manifests.ts`**: Both set `additionalOptions: true` on their default create option. Language and member-group creation navigate directly to a workspace form (no intermediate dialog), so `additionalOptions: true` does not match the documented intent ("a dialog will prompt the user for more information or to make a choice"). If the intent is just to show `...` as a visual affordance, this is a misuse of the flag. Alternatively, if the workspace IS considered "additional options", add a comment explaining the intentional interpretation.

---

## Approved with Suggestions for improvement

Good to go, but please carefully consider the importance of the suggestions.
