import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

const IDENTIFIER_PREFIX = 'UMB_LANGUAGE_PERMISSION_';

export class UmbBlockLanguageAccessWorkspaceContext extends UmbControllerBase {
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#variantId?: UmbVariantId;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(
				instance?.variantId,
				(variantId) => {
					this.#variantId = variantId;
					this.#checkForLanguageAccess();
				},
				'observeBlockVariantId',
			);
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.languages,
				(languages) => {
					this.#currentUserAllowedLanguages = languages;
					this.#checkForLanguageAccess();
				},
				'observeCurrentUserLanguages',
			);

			this.observe(
				context?.hasAccessToAllLanguages,
				(hasAccessToAllLanguages) => {
					this.#currentUserHasAccessToAllLanguages = hasAccessToAllLanguages;
					this.#checkForLanguageAccess();
				},
				'observeCurrentUserHasAccessToAllLanguages',
			);
		});
	}

	#checkForLanguageAccess() {
		if (!this.#workspaceContext) return;

		const culture = this.#variantId?.culture ?? undefined;

		// If the block is invariant/segment-only, or the user has access to all languages,
		// there is no language-based restriction to apply.
		const allowed =
			!culture || this.#currentUserHasAccessToAllLanguages === true
				? true
				: (this.#currentUserAllowedLanguages?.includes(culture) ?? false);

		const unique = IDENTIFIER_PREFIX + culture;

		// Remove any previous rule before potentially adding a new one, so switching
		// the block's culture does not leave a stale rule from the previous variant.
		this.#workspaceContext.readOnlyGuard.removeRule(unique);
		this.#workspaceContext.content.readOnlyGuard.removeRule(unique);
		this.#workspaceContext.settings.readOnlyGuard.removeRule(unique);

		if (allowed || !culture || !this.#variantId) return;

		const variantId = this.#variantId;
		const rule = {
			unique,
			variantId,
			message: 'You do not have permission to edit this culture',
			// The rule semantics match the document workspace version:
			// permitted: true = the variant is permitted to be read-only.
			permitted: true,
		};

		this.#workspaceContext.readOnlyGuard.addRule(rule);
		this.#workspaceContext.content.readOnlyGuard.addRule(rule);
		this.#workspaceContext.settings.readOnlyGuard.addRule(rule);
	}
}

export { UmbBlockLanguageAccessWorkspaceContext as api };
