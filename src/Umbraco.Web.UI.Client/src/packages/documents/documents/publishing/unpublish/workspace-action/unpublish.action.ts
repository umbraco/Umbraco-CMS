import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../../workspace-context/constants.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentUnpublishWorkspaceAction extends UmbWorkspaceActionBase {
	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('Publishing workspace context not found');
		}
		return workspaceContext.unpublish();
	}
}

export { UmbDocumentUnpublishWorkspaceAction as api };
