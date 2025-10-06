import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/document-workspace.context-token.js';
import type { ManifestWorkspaceActionMenuItemUrlProviderKind } from './url-provider.workspace-action-menu-item.extension.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveAndPreviewWorkspaceAction extends UmbWorkspaceActionBase {
	manifest?: ManifestWorkspaceActionMenuItemUrlProviderKind;

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('The workspace context is missing');
		}
		workspaceContext?.saveAndPreview(this.manifest?.urlProviderAlias);
	}
}

export { UmbDocumentSaveAndPreviewWorkspaceAction as api };
