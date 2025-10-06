import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/document-workspace.context-token.js';
import { UmbPreviewOptionActionBase } from './preview-option-action-base.controller.js';
import type { ManifestPreviewOptionUrlProviderKind } from './preview-option.extension.js';

export class UmbUrlProviderPreviewOptionAction extends UmbPreviewOptionActionBase {
	manifest?: ManifestPreviewOptionUrlProviderKind;

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		workspaceContext?.saveAndPreview();
	}
}

export { UmbUrlProviderPreviewOptionAction as api };
