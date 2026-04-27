import { UMB_BLOCK_MANAGER_CONTEXT } from '../context/block-manager.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

const IDENTIFIER_PREFIX = 'UMB_LANGUAGE_PERMISSION_';

export class UmbBlockLanguageAccessWorkspaceController extends UmbControllerBase {
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#variantId?: UmbVariantId;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;
	#consumeBlockManager?: UmbContextConsumerController<typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.readOnlyGuard.fallbackToPermitted();
			this.#workspaceContext?.content.readOnlyGuard.fallbackToPermitted();
			this.#workspaceContext?.settings.readOnlyGuard.fallbackToPermitted();

			this.observe(
				instance?.variantId,
				(variantId) => {
					this.#variantId = variantId;

					this.#observeBlockManager(variantId);

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

	#observeBlockManager(variantId?: UmbVariantId) {
		if (variantId?.isCultureInvariant()) {
			/**
			 * If the Block Workspace is invariant, the readOnly state from the Block Manager should apply to the invariant fields(all) of this Workspace: [NL]
			 */
			this.#consumeBlockManager = this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
				this.observe(
					manager?.readOnlyState.permitted,
					(isReadOnly) => {
						const unique = 'UMB_BLOCK_MANAGER_CONTEXT';

						if (isReadOnly) {
							this.#workspaceContext?.readOnlyGuard.removeRule(unique);
							this.#workspaceContext?.content.readOnlyGuard.removeRule(unique);
							this.#workspaceContext?.settings.readOnlyGuard.removeRule(unique);
						} else {
							const rule = {
								unique,
								permitted: false,
								variantId: UmbVariantId.INVARIANT,
							};

							this.#workspaceContext?.readOnlyGuard.addRule(rule);
							this.#workspaceContext?.content.readOnlyGuard.addRule(rule);
							this.#workspaceContext?.settings.readOnlyGuard.addRule(rule);
						}
					},
					'observeManagerReadOnly',
				);
			});
		} else {
			this.#workspaceContext?.readOnlyGuard.removeRule('UMB_BLOCK_MANAGER_CONTEXT');
			this.#workspaceContext?.content.readOnlyGuard.removeRule('UMB_BLOCK_MANAGER_CONTEXT');
			this.#workspaceContext?.settings.readOnlyGuard.removeRule('UMB_BLOCK_MANAGER_CONTEXT');
			this.#consumeBlockManager?.destroy();
			this.#consumeBlockManager = undefined;
			this.removeUmbControllerByAlias('observeManagerReadOnly');
		}
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

		if (!allowed || !culture || !this.#variantId) return;

		const variantId = this.#variantId;
		const rule = {
			unique,
			variantId,
			// The rule semantics match the document workspace version:
			// permitted: false = the variant is permitted to be edited.
			permitted: false,
		};

		this.#workspaceContext.readOnlyGuard.addRule(rule);
		this.#workspaceContext.content.readOnlyGuard.addRule(rule);
		this.#workspaceContext.settings.readOnlyGuard.addRule(rule);
	}
}

export { UmbBlockLanguageAccessWorkspaceController as api };
