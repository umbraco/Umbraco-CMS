import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../constants.js';
import { UmbDocumentAllowEditInvariantFromNonDefaultControllerBase } from './document-allow-edit-invariant-from-non-default-controller-base.js';
import { UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT } from '@umbraco-cms/backoffice/property';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';

export class UmbDocumentBlockWorkspaceAllowEditInvariantFromNonDefaultController extends UmbDocumentAllowEditInvariantFromNonDefaultControllerBase {
	protected async _preventEditInvariantFromNonDefault() {
		const varyingPropertyContext = await this.getContext(UMB_PROPERTY_CONTEXT_FOR_CULTURE_VARIANT, {
			passContextAliasMatches: true,
		});

		console.log('varyingPropertyContext', varyingPropertyContext);

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
	}
}

export { UmbDocumentBlockWorkspaceAllowEditInvariantFromNonDefaultController as api };
