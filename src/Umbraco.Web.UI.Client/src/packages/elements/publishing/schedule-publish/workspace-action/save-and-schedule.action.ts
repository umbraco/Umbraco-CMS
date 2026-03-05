import { UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../../workspace-context/constants.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbElementSaveAndScheduleWorkspaceAction extends UmbWorkspaceActionBase {
	override async execute() {
		const workspaceContext = await this.getContext(UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('Publishing workspace context not found');
		}
		return workspaceContext.schedule();
	}
}

export { UmbElementSaveAndScheduleWorkspaceAction as api };
