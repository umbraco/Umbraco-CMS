import { UMB_BLOCK_MANAGER_CONTEXT } from '../context/block-manager.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

const IDENTIFIER_PREFIX = 'UMB_LANGUAGE_PERMISSION_';

/**
 * Configures the read-only state of a Block Workspace based on the parent Block Manager
 * and the current user's language access.
 *
 * - For invariant blocks, the workspace inherits the read-only state of the parent Block Manager(Host Property).
 * - For variant blocks (with a culture), the workspace is editable only when the current user
 *   has access to that culture (either via `hasAccessToAllLanguages` or an entry in their
 *   allowed languages).
 */
export class UmbBlockLanguageAccessWorkspaceController extends UmbControllerBase {
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#variantId?: UmbVariantId;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;
	#consumeBlockManager?: UmbContextConsumerController<typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE>;
	#appliedLanguageUnique?: string;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.readOnlyGuard.fallbackToNotPermitted();
			this.#workspaceContext?.content.readOnlyGuard.fallbackToNotPermitted();
			this.#workspaceContext?.settings.readOnlyGuard.fallbackToNotPermitted();

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
		const unique = 'UMB_BLOCK_MANAGER_CONTEXT';
		if (variantId?.isCultureInvariant()) {
			/**
			 * If the Block Workspace is invariant, the readOnly state from the Block Manager should apply to the invariant fields(all) of this Workspace: [NL]
			 */
			// Destroy any prior consumer before reassigning, so a re-emit of an invariant
			// variantId does not leak the previous context consumer. [NL]
			this.#consumeBlockManager?.destroy();
			this.#consumeBlockManager = this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
				this.observe(
					manager?.readOnlyState.permitted,
					(isReadOnly) => {
						if (isReadOnly === undefined) return;

						if (isReadOnly) {
							const rule = {
								unique,
								permitted: true,
							};

							this.#workspaceContext?.readOnlyGuard.addRule(rule);
							this.#workspaceContext?.content.readOnlyGuard.addRule(rule);
							this.#workspaceContext?.settings.readOnlyGuard.addRule(rule);
						} else {
							this.#workspaceContext?.readOnlyGuard.removeRule(unique);
							this.#workspaceContext?.content.readOnlyGuard.removeRule(unique);
							this.#workspaceContext?.settings.readOnlyGuard.removeRule(unique);
						}
					},
					'observeManagerReadOnly',
				);
			});
		} else {
			this.#workspaceContext?.readOnlyGuard.removeRule(unique);
			this.#workspaceContext?.content.readOnlyGuard.removeRule(unique);
			this.#workspaceContext?.settings.readOnlyGuard.removeRule(unique);
			this.#consumeBlockManager?.destroy();
			this.#consumeBlockManager = undefined;
			this.removeUmbControllerByAlias('observeManagerReadOnly');
		}
	}

	#checkForLanguageAccess() {
		if (
			!this.#workspaceContext ||
			this.#currentUserHasAccessToAllLanguages == undefined ||
			this.#currentUserAllowedLanguages == undefined
		) {
			return;
		}

		const culture = this.#variantId?.culture ?? undefined;

		// If the block is invariant/segment-only, or the user has access to all languages,
		// there is no language-based restriction to apply.
		const allowed =
			!culture || this.#currentUserHasAccessToAllLanguages === true
				? true
				: (this.#currentUserAllowedLanguages?.includes(culture) ?? false);

		// Always remove the previously applied rule (tracked by the actual unique key,
		// not just the current culture). Without this, switching the workspace's culture
		// from A → B leaves a stale UMB_LANGUAGE_PERMISSION_<A> rule lingering in the
		// guard manager — `findRule()` is variant-scoped so it stays harmless, but
		// `getRules()` accumulates one entry per visited culture over the workspace's
		// lifetime. [NL]
		if (this.#appliedLanguageUnique) {
			this.#workspaceContext.readOnlyGuard.removeRule(this.#appliedLanguageUnique);
			this.#workspaceContext.content.readOnlyGuard.removeRule(this.#appliedLanguageUnique);
			this.#workspaceContext.settings.readOnlyGuard.removeRule(this.#appliedLanguageUnique);
			this.#appliedLanguageUnique = undefined;
		}

		if (allowed || !culture || !this.#variantId) return;

		const variantId = this.#variantId;
		const unique = IDENTIFIER_PREFIX + culture;
		const rule = {
			unique,
			variantId,
			// `permitted: true` on a read-only guard means "to be read-only"
			// — i.e. not editable. Combined with `fallbackToPermitted()` (default = read-only).
			permitted: true,
		};

		this.#workspaceContext.readOnlyGuard.addRule(rule);
		this.#workspaceContext.content.readOnlyGuard.addRule(rule);
		this.#workspaceContext.settings.readOnlyGuard.addRule(rule);
		this.#appliedLanguageUnique = unique;
	}
}

export { UmbBlockLanguageAccessWorkspaceController as api };
