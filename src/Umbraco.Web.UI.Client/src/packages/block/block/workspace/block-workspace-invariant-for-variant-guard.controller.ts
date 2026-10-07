import { UMB_BLOCK_MANAGER_CONTEXT } from '../context/block-manager.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './context/block-workspace.context-token.js';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT } from '@umbraco-cms/backoffice/property';
import {
	UMB_CONTENT_WORKSPACE_CONTEXT,
	UmbContentInvariantForVariantGuardControllerBase,
} from '@umbraco-cms/backoffice/content';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { observeMultiple, type Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockWorkspaceInvariantForVariantGuardController extends UmbContentInvariantForVariantGuardControllerBase {
	protected async _preventEditInvariantForVariant() {
		//
		const varyingProperty = await this.getContext(UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT, {
			passContextAliasMatches: true,
		}).catch(() => undefined);
		if (varyingProperty) {
			// If we have a varying property context, we can assume we are in a culture variant branch and therefore we can assume the property is allowed to be edited.
			return;
		}

		const contentWorkspaceContext = await this.getContext(UMB_CONTENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		}).catch(() => undefined);

		// Blocks are not necessarily hosted by a content workspace, in which case there is no variant content to guard.
		if (!contentWorkspaceContext) return;

		const blockWorkspace = await this.getContext(UMB_BLOCK_WORKSPACE_CONTEXT);
		if (!blockWorkspace) {
			throw new Error('Missing Block Workspace Context');
		}

		// --- Rule for invariant fields in variant blocks edited via a non-default variant ---

		// Existing rules for variant blocks (where datasetVariantId matches the viewing language)
		this._observeAndApplyRule({
			propertiesObservable: blockWorkspace.content.structure.contentTypeProperties,
			variantOptionsObservable: contentWorkspaceContext.variantOptions,
			variesByCultureObservable: contentWorkspaceContext.structure.variesByCulture,
			propertyWriteGuard: blockWorkspace.content.propertyWriteGuard,
		});
		this._observeAndApplyRule({
			propertiesObservable: blockWorkspace.settings.structure.contentTypeProperties,
			variantOptionsObservable: contentWorkspaceContext.variantOptions,
			variesByCultureObservable: contentWorkspaceContext.structure.variesByCulture,
			propertyWriteGuard: blockWorkspace.settings.propertyWriteGuard,
		});

		// --- Rule for fields inside invariant blocks viewed from a non-default language ---

		/** Note because this rule only applies in invariant contexts, then we can use the nearest Block Manager to see what the active variant is. In a data-branch of a mix between varying and not-varying data this would not work [NL] */
		const blockManager = await this.getContext(UMB_BLOCK_MANAGER_CONTEXT);

		if (!blockManager) {
			throw new Error('Missing Block Manager Context');
		}

		// Additional rules for invariant blocks (where datasetVariantId is invariant)
		// These blocks don't vary by culture, so their datasetVariantId is always invariant,
		// but we still need to prevent editing when viewing from a non-default language.
		this.#applyRuleForInvariantBlocks(
			blockManager.variantId,
			blockWorkspace.content.structure.variesByCulture,
			contentWorkspaceContext.structure.variesByCulture,
			contentWorkspaceContext.variantOptions,
			blockWorkspace.content.propertyWriteGuard,
		);
		this.#applyRuleForInvariantBlocks(
			blockManager.variantId,
			blockWorkspace.settings.structure.variesByCulture,
			contentWorkspaceContext.structure.variesByCulture,
			contentWorkspaceContext.variantOptions,
			blockWorkspace.settings.propertyWriteGuard,
		);
	}

	// eslint-disable-next-line jsdoc/require-param
	/**
	 * For invariant element types, the block's datasetVariantId is always invariant (null culture).
	 * The existing rules target non-default language datasetVariantIds (e.g., da-DK), which don't match.
	 * This method adds a rule with invariant datasetVariantId when viewing from a non-default language.
	 */
	#applyRuleForInvariantBlocks(
		managerVariantId: Observable<UmbVariantId | undefined>,
		variesByCulture: Observable<boolean | undefined>,
		ownerVariesByCulture: Observable<boolean | undefined>,
		variantOptions: Observable<Array<UmbEntityVariantOptionModel>>,
		propertyWriteGuard: UmbVariantPropertyGuardManager,
	) {
		this._observeWhileRestricted(
			observeMultiple([managerVariantId, variesByCulture, ownerVariesByCulture, variantOptions]),
			([managerVariantId, variesByCulture, ownerVariesByCulture, variantOptions]) => {
				// Only apply for invariant element types (blocks that don't vary by culture)
				if (variesByCulture !== false) return;
				if (!managerVariantId || !variantOptions.length) return;

				// If the owning content itself doesn't vary by culture, there is no "non-default language"
				// to view from - the invariant-for-variant permission is meaningless here and must not restrict it.
				if (ownerVariesByCulture === false) return;

				// The user lacks the invariant-for-variant permission, so apply the rule for invariant
				// blocks regardless of which variant tab is being viewed (default language included).
				this._addRestrictionRule(
					propertyWriteGuard,
					this._createRule({ datasetVariantId: UmbVariantId.CreateInvariant() }),
				);
			},
		);
	}
}

export { UmbBlockWorkspaceInvariantForVariantGuardController as api };
