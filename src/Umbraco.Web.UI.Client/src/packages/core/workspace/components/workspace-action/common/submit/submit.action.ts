import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../../../../contexts/tokens/index.js';
import type { UmbSubmittableWorkspaceContext } from '../../../../contexts/tokens/index.js';
import type { UmbWorkspaceActionArgs } from '../../types.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
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
					this.disable();
				} else {
					this.enable();
				}
			},
			'saveWorkspaceActionUniqueObserver',
		);
	}

	override async execute() {
		const workspaceContext = await this.getContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT);
		return await workspaceContext.requestSubmit();
	}
}

/*
 * @deprecated Use UmbSubmitWorkspaceAction instead
 */
export { UmbSubmitWorkspaceAction as UmbSaveWorkspaceAction };
