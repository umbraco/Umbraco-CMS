import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../constants.js';
import { UmbDocumentAllowEditInvariantFromNonDefaultControllerBase } from './document-allow-edit-invariant-from-non-default-controller-base.js';
import { UMB_PROPERTY_DATASET_CONTEXT, type UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';

export class UmbDocumentBlockWorkspaceAllowEditInvariantFromNonDefaultController extends UmbDocumentAllowEditInvariantFromNonDefaultControllerBase {
	protected async _preventEditInvariantFromNonDefault() {
		const documentWorkspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		});

		if (!documentWorkspaceContext) {
			throw new Error('Missing Document Workspace Context');
		}

		const blockWorkspaceContext = await this.getContext(UMB_BLOCK_WORKSPACE_CONTEXT);

		if (!blockWorkspaceContext) {
			throw new Error('Missing Block Workspace Context');
		}

		const blockOwnerContentType = blockWorkspaceContext.content.structure.getOwnerContentType();
		const blockVariesByCulture = blockOwnerContentType?.variesByCulture;

		const propertyDatasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);

		if (!blockWorkspaceContext) {
			throw new Error('Missing Property Dataset Context');
		}

		const datasetVariantId = propertyDatasetContext?.getVariantId();
		const variantOptions = await this.observe(documentWorkspaceContext.variantOptions).asPromise();
		const datasetVariantOption = variantOptions.filter((variantOption) => datasetVariantId?.compare(variantOption));
		const isDatasetDefaultLanguage =
			datasetVariantOption?.length > 0 ? datasetVariantOption[0].language.isDefault : false;

		// Prevent editing invariant blocks when editing from the non default language
		if (blockVariesByCulture === false && isDatasetDefaultLanguage === false) {
			const datasetVariantId = UmbVariantId.CreateInvariant();
			const rule: UmbVariantPropertyGuardRule = this._createRule({ datasetVariantId });

			blockWorkspaceContext?.content.propertyWriteGuard.addRule(rule);
		} else {
			this._observeAndApplyRule({
				propertiesObservable: blockWorkspaceContext.content.structure.contentTypeProperties,
				variantOptionsObservable: documentWorkspaceContext.variantOptions,
				propertyWriteGuard: blockWorkspaceContext?.content.propertyWriteGuard,
			});
		}
	}
}

export { UmbDocumentBlockWorkspaceAllowEditInvariantFromNonDefaultController as api };
