import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../constants.js';
import { UmbDocumentAllowEditInvariantFromNonDefaultControllerBase } from './document-allow-edit-invariant-from-non-default-controller-base.js';

export class UmbDocumentWorkspaceAllowEditInvariantFromNonDefaultController extends UmbDocumentAllowEditInvariantFromNonDefaultControllerBase {
	protected async _preventEditInvariantFromNonDefault() {
		const documentWorkspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		});

		if (!documentWorkspaceContext) {
			throw new Error('Missing Document Workspace Context');
		}

		this._observeAndApplyRule({
			propertiesObservable: documentWorkspaceContext.structure.contentTypeProperties,
			variantOptionsObservable: documentWorkspaceContext.variantOptions,
			propertyWriteGuard: documentWorkspaceContext.propertyWriteGuard,
		});
	}
}

export { UmbDocumentWorkspaceAllowEditInvariantFromNonDefaultController as api };
