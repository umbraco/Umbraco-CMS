import type { UmbSaveableWorkspaceContext } from '../../../../contexts/tokens/saveable-workspace-context.interface.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
import { UMB_SAVEABLE_WORKSPACE_CONTEXT, type UmbWorkspaceActionArgs } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSaveWorkspaceAction extends UmbWorkspaceActionBase<UmbSaveableWorkspaceContext> {
	#workspaceContext?: UmbSaveableWorkspaceContext;

	constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<UmbSaveableWorkspaceContext>) {
		super(host, args);

		// TODO: Could we make change label depending on the state?
		this.consumeContext(UMB_SAVEABLE_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeUnique();
		});
	}

	#observeUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				// We can't save if we don't have a unique
				if (unique === undefined) {
					this._isDisabled.setValue(true);
				} else {
					this._isDisabled.setValue(false);
				}
			},
			'saveWorkspaceActionUniqueObserver',
		);
	}

	async execute() {
		const workspaceContext = await this.getContext(UMB_SAVEABLE_WORKSPACE_CONTEXT);
		return workspaceContext.save();
	}
}
