import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../constants.js';
import { UmbDocumentAllowEditInvariantFromNonDefaultControllerBase } from './document-allow-edit-invariant-from-non-default-controller-base.js';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT } from '@umbraco-cms/backoffice/property';
import { UMB_BLOCK_WORKSPACE_CONTEXT, UMB_BLOCK_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { observeMultiple, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbDocumentVariantModel } from '../types.js';

export class UmbDocumentBlockWorkspaceAllowEditInvariantFromNonDefaultController extends UmbDocumentAllowEditInvariantFromNonDefaultControllerBase {
	protected async _preventEditInvariantFromNonDefault() {
		const varyingPropertyContext = await this.getContext(UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT, {
			passContextAliasMatches: true,
		}).catch(() => undefined);

		if (varyingPropertyContext) {
			// If we have a varying property context, we can assume we are in a culture variant branch and therefor we can assume the property is allowed to be edited.
			return;
		}

		const documentWorkspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		});

		if (!documentWorkspaceContext) {
			throw new Error('Missing Document Workspace Context');
		}

		const blockWorkspaceContext = await this.getContext(UMB_BLOCK_WORKSPACE_CONTEXT);

		// Note, what if its two levels away?
		if (!blockWorkspaceContext) {
			throw new Error('Missing Block Workspace Context');
		}

		const blockManager = await this.getContext(UMB_BLOCK_MANAGER_CONTEXT).catch(() => undefined);

		// Existing rules for variant blocks (where datasetVariantId matches the viewing language)
		this._observeAndApplyRule({
			propertiesObservable: blockWorkspaceContext.content.structure.contentTypeProperties,
			variantOptionsObservable: documentWorkspaceContext.variantOptions,
			propertyWriteGuard: blockWorkspaceContext?.content.propertyWriteGuard,
		});
		this._observeAndApplyRule({
			propertiesObservable: blockWorkspaceContext.settings.structure.contentTypeProperties,
			variantOptionsObservable: documentWorkspaceContext.variantOptions,
			propertyWriteGuard: blockWorkspaceContext?.settings.propertyWriteGuard,
		});

		// Additional rules for invariant blocks (where datasetVariantId is invariant)
		// These blocks don't vary by culture, so their datasetVariantId is always invariant,
		// but we still need to prevent editing when viewing from a non-default language.
		if (blockManager) {
			this.#applyRuleForInvariantBlocks(
				blockManager.variantId,
				blockWorkspaceContext.content.structure.variesByCulture,
				documentWorkspaceContext.variantOptions,
				blockWorkspaceContext.content.propertyWriteGuard,
			);
			this.#applyRuleForInvariantBlocks(
				blockManager.variantId,
				blockWorkspaceContext.settings.structure.variesByCulture,
				documentWorkspaceContext.variantOptions,
				blockWorkspaceContext.settings.propertyWriteGuard,
			);
		}
	}

	/**
	 * For invariant element types, the block's datasetVariantId is always invariant (null culture).
	 * The existing rules target non-default language datasetVariantIds (e.g., da-DK), which don't match.
	 * This method adds a rule with invariant datasetVariantId when viewing from a non-default language.
	 */
	#applyRuleForInvariantBlocks(
		managerVariantId$: Observable<UmbVariantId | undefined>,
		variesByCulture$: Observable<boolean | undefined>,
		variantOptions$: Observable<UmbEntityVariantOptionModel<UmbDocumentVariantModel>[]>,
		propertyWriteGuard: UmbVariantPropertyGuardManager,
	) {
		this.observe(
			observeMultiple([managerVariantId$, variesByCulture$, variantOptions$]),
			([managerVariantId, variesByCulture, variantOptions]) => {
				// Only apply for invariant element types (blocks that don't vary by culture)
				if (variesByCulture !== false) return;
				if (!managerVariantId || !variantOptions.length) return;

				// Check if we're viewing a non-default language
				const currentOption = variantOptions.find((v) => v.culture === managerVariantId.culture);
				if (!currentOption || currentOption.language.isDefault) return;

				// Add rule for invariant datasetVariantId
				const rule = this._createRule({ datasetVariantId: UmbVariantId.CreateInvariant() });
				propertyWriteGuard.addRule(rule);
			},
		);
	}
}

export { UmbDocumentBlockWorkspaceAllowEditInvariantFromNonDefaultController as api };
