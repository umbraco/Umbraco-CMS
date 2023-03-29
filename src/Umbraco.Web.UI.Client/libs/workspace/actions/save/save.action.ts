import { UmbWorkspaceContextInterface } from '../../context/workspace-context.interface';
import { UmbWorkspaceActionBase } from '../workspace-action-base';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

// TODO: add interface for repo/partial repo/save-repo
export class UmbSaveWorkspaceAction extends UmbWorkspaceActionBase<UmbWorkspaceContextInterface> {
	constructor(host: UmbControllerHostElement) {
		super(host);
	}

	/* TODO: we need a solution for all actions to notify the system that is has been executed.
		There might be cases where we need to do something after the action has been executed.
		Ex. "reset" a workspace after a save action has been executed.
	*/
	async execute() {
		if (!this.workspaceContext) return;
		// TODO: it doesn't get the updated value
		const data = this.workspaceContext.getData();
		// TODO: handle errors
		if (!data) return;

		this.workspaceContext.getIsNew() ? this.#create(data) : this.#update(data);
	}

	async #create(data: any) {
		if (!this.workspaceContext) return;

		// TODO: preferably the actions dont talk directly with repository, but instead with its context.
		// We just need to consider how third parties can extend the system.
		const { error } = await this.workspaceContext.repository.create(data);

		// TODO: this is temp solution to bubble validation errors to the UI
		if (error) {
			if (error.type === 'validation') {
				this.workspaceContext.setValidationErrors?.(error.errors);
			}
		} else {
			this.workspaceContext.setValidationErrors?.(undefined);
			// TODO: do not make it the buttons responsibility to set the workspace to not new.
			this.workspaceContext.setIsNew(false);
		}
	}

	#update(data: any) {
		if (!this.workspaceContext) return;
		this.workspaceContext.repository?.save(data);
	}
}
