import type { UmbSubmittableWorkspaceContext } from '../../../../contexts/tokens/submittable-workspace-context.interface.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT, type UmbWorkspaceActionArgs } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSubmitWorkspaceAction extends UmbWorkspaceActionBase<UmbSubmittableWorkspaceContext> {
	#workspaceContext?: UmbSubmittableWorkspaceContext;

	constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<UmbSubmittableWorkspaceContext>) {
		super(host, args);

		// TODO: Could we make change label depending on the state?
		this.consumeContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
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
		const workspaceContext = await this.getContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT);
		return await workspaceContext.requestSubmit();
	}
}

/*
 * @deprecated Use UmbSubmitWorkspaceAction instead
 * TODO: Remove as part of RC
 */
export { UmbSubmitWorkspaceAction as UmbSaveWorkspaceAction };
