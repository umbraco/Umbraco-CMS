import { UmbWorkspaceContextInterface } from '../../components/workspace/workspace-context/workspace-context.interface';
import { UmbWorkspaceActionBase } from '../../components/workspace/workspace-action/workspace-action-base';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

// TODO: add interface for repo/partial repo/save-repo
export class UmbSaveWorkspaceAction extends UmbWorkspaceActionBase<UmbWorkspaceContextInterface> {
	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: Could we make change label depending on the state?
		// So its called 'Create' when the workspace is new and 'Save' when the workspace is not new.
	}

	/* TODO: we need a solution for all actions to notify the system that is has been executed.
		There might be cases where we need to do something after the action has been executed.
		Ex. "reset" a workspace after a save action has been executed.
	*/
	async execute() {
		if (!this.workspaceContext) return;
		this.workspaceContext.save();
	}
}
