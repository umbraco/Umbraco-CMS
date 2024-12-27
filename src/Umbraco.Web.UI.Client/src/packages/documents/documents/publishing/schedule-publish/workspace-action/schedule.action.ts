import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../../workspace-context/constants.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentScheduleWorkspaceAction extends UmbWorkspaceActionBase {
	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT);
		return workspaceContext.schedule();
	}
}

export { UmbDocumentScheduleWorkspaceAction as api };
