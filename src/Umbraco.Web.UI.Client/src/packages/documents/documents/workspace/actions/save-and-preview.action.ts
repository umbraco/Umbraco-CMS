import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../document-workspace.context-token.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveAndPreviewWorkspaceAction extends UmbWorkspaceActionBase {
	async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		//const document = workspaceContext.getData();
		// TODO: handle errors
		//if (!document) return;
		//this.workspaceContext.repository?.saveAndPreview();
		//Remember to return a promise.
	}
}
