import type { UmbSaveableWorkspaceContextInterface } from '../../../../contexts/saveable-workspace-context.interface.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
import { UMB_SAVEABLE_WORKSPACE_CONTEXT, type UmbWorkspaceActionArgs } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSaveWorkspaceAction extends UmbWorkspaceActionBase<UmbSaveableWorkspaceContextInterface> {
	constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<UmbSaveableWorkspaceContextInterface>) {
		super(host, args);

		// TODO: Could we make change label depending on the state?
	}

	async execute() {
		const workspaceContext = await this.getContext(UMB_SAVEABLE_WORKSPACE_CONTEXT);
		return workspaceContext.save();
	}
}
