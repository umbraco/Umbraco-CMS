import { UMB_BLOCK_MANAGER_CONTEXT } from '../context/block-manager.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT } from '@umbraco-cms/backoffice/property';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

const IDENTIFIER_PREFIX = 'UMB_LANGUAGE_PERMISSION_';
const INVARIANT_PROPERTY_WRITE_RULE_PREFIX = 'UMB_LANGUAGE_PERMISSION_INVARIANT_PROPERTY_';
const INVARIANT_WRITE_DENIED_MESSAGE =
	'You do not have permission to edit shared (invariant) properties on this content.';

/**
 * Configures the read-only state of a Block Workspace based on the parent Block Manager
 * and the current user's language access.
 *
 * - For invariant blocks, the workspace inherits the read-only state of the parent Block Manager(Host Property).
 * - For variant blocks (with a culture), the workspace is editable only when the current user
 *   has access to that culture (either via `hasAccessToAllLanguages` or an entry in their
 *   allowed languages).
 *
 * Without the invariant-for-variant permission, shared (invariant) properties of the block are also
 * read-only, unless the block is hosted by a property that varies by culture - the block content then
 * belongs to that culture and only language access applies.
 */
export class UmbBlockLanguageAccessWorkspaceController extends UmbControllerBase {
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#variantId?: UmbVariantId;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;
	#consumeBlockManager?: UmbContextConsumerController<typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE>;
	#appliedLanguageUnique?: string;
	#currentUserHasAccessToInvariantForVariant?: boolean;
	#hostedByVaryingProperty = false;
	#ownerWorkspace?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	#ownerVariesByCulture?: boolean;
	#ownerVariantOptions: Array<UmbEntityVariantOptionModel> = [];
	#managerVariantId?: UmbVariantId;
	#contentVariesByCulture?: boolean;
	#settingsVariesByCulture?: boolean;
	#invariantRules: Array<{ guard: UmbVariantPropertyGuardManager; unique: string }> = [];

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

			this.observe(
				instance?.content.structure.variesByCulture,
				(variesByCulture) => {
					this.#contentVariesByCulture = variesByCulture;
					this.#restrictInvariantData();
				},
				'observeBlockContentVariesByCulture',
			);

			this.observe(
				instance?.settings.structure.variesByCulture,
				(variesByCulture) => {
					this.#settingsVariesByCulture = variesByCulture;
					this.#restrictInvariantData();
				},
				'observeBlockSettingsVariesByCulture',
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

			this.observe(
				context?.hasAccessToInvariantForVariant,
				(hasAccessToInvariantForVariant) => {
					this.#currentUserHasAccessToInvariantForVariant = hasAccessToInvariantForVariant;
					this.#restrictInvariantData();
				},
				'observeCurrentUserHasAccessToInvariantForVariant',
			);
		});

		this.consumeContext(UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT, (propertyContext) => {
			this.#hostedByVaryingProperty = propertyContext !== undefined;
			this.#restrictInvariantData();
		}).passContextAliasMatches();

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (ownerWorkspace) => {
			this.#ownerWorkspace = ownerWorkspace;

			this.observe(
				ownerWorkspace?.variantOptions,
				(variantOptions) => {
					this.#ownerVariantOptions = variantOptions ?? [];
					this.#restrictInvariantData();
				},
				'observeOwnerVariantOptions',
			);

			this.observe(
				ownerWorkspace?.structure.variesByCulture,
				(variesByCulture) => {
					this.#ownerVariesByCulture = variesByCulture;
					this.#restrictInvariantData();
				},
				'observeOwnerVariesByCulture',
			);
		}).passContextAliasMatches();

		this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
			this.observe(
				manager?.variantId,
				(variantId) => {
					this.#managerVariantId = variantId;
					this.#restrictInvariantData();
				},
				'observeBlockManagerVariantId',
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

	/**
	 * Without the invariant-for-variant permission, shared data of a block inside content that varies by culture is
	 * read-only on every variant. Blocks hosted by a culture-varying property are exempt: their content is culture data.
	 * Rules are re-evaluated from scratch on every change, so nothing lingers when a precondition stops holding.
	 */
	#restrictInvariantData() {
		this.#clearInvariantRules();

		const workspace = this.#workspaceContext;
		if (!workspace || !this.#ownerWorkspace) return;
		if (this.#currentUserHasAccessToInvariantForVariant !== false) return;
		if (this.#hostedByVaryingProperty) return;
		if (this.#ownerVariesByCulture === false) return;
		if (this.#ownerVariantOptions.length === 0) return;

		const guards = [
			{ guard: workspace.content.propertyWriteGuard, variesByCulture: this.#contentVariesByCulture },
			{ guard: workspace.settings.propertyWriteGuard, variesByCulture: this.#settingsVariesByCulture },
		];

		for (const { guard, variesByCulture } of guards) {
			for (const variantOption of this.#ownerVariantOptions) {
				this.#addInvariantRule(guard, UmbVariantId.CreateFromPartial(variantOption));
			}

			// Blocks of an element type that does not vary by culture have an invariant dataset, which the rules above never match.
			if (variesByCulture === false && this.#managerVariantId) {
				this.#addInvariantRule(guard, UmbVariantId.CreateInvariant());
			}
		}
	}

	#addInvariantRule(guard: UmbVariantPropertyGuardManager, datasetVariantId: UmbVariantId) {
		const unique = INVARIANT_PROPERTY_WRITE_RULE_PREFIX + datasetVariantId.toString();
		guard.addRule({
			unique,
			message: INVARIANT_WRITE_DENIED_MESSAGE,
			variantId: UmbVariantId.CreateInvariant(),
			datasetVariantId,
			permitted: false,
		});
		this.#invariantRules.push({ guard, unique });
	}

	#clearInvariantRules() {
		this.#invariantRules.forEach(({ guard, unique }) => guard.removeRule(unique));
		this.#invariantRules = [];
	}
}

export { UmbBlockLanguageAccessWorkspaceController as api };
