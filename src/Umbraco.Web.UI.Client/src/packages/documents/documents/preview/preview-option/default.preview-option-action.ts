import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/document-workspace.context-token.js';
import { UmbPreviewOptionActionBase } from './preview-option-action-base.controller.js';

export class UmbDefaultPreviewOptionAction extends UmbPreviewOptionActionBase {
	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		workspaceContext?.saveAndPreview();
	}
}

export { UmbDefaultPreviewOptionAction as api };
