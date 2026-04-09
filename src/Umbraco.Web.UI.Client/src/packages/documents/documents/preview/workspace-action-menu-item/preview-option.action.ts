import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/document-workspace.context-token.js';
import type { ManifestWorkspaceActionMenuItemPreviewOptionKind } from './preview-option.workspace-action-menu-item.extension.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveAndPreviewOptionWorkspaceAction extends UmbWorkspaceActionBase {
	manifest?: ManifestWorkspaceActionMenuItemPreviewOptionKind;

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('The workspace context is missing');
		}
		await workspaceContext?.saveAndPreview(this.manifest?.meta.urlProviderAlias);
	}
}

export { UmbDocumentSaveAndPreviewOptionWorkspaceAction as api };
