import type { MetaWorkspaceAction } from '../../../../types.js';
import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../../../../contexts/tokens/index.js';
import type { UmbSubmittableWorkspaceContext } from '../../../../contexts/tokens/index.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
import type { UmbSubmitWorkspaceActionArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSubmitWorkspaceAction<
	ArgsMetaType extends MetaWorkspaceAction = MetaWorkspaceAction,
	WorkspaceContextType extends UmbSubmittableWorkspaceContext = UmbSubmittableWorkspaceContext,
> extends UmbWorkspaceActionBase<ArgsMetaType> {
	protected _init: Promise<unknown>;
	protected _workspaceContext?: WorkspaceContextType;

	constructor(host: UmbControllerHost, args: UmbSubmitWorkspaceActionArgs<ArgsMetaType>) {
		super(host, args);

		// TODO: Could we make change label depending on the state? [NL]
		this._init = this.consumeContext(args.workspaceContextToken ?? UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context as WorkspaceContextType;
			this.#observeUnique();
			this._gotWorkspaceContext();
		}).asPromise();
	}

	#observeUnique() {
		this.observe(
			this._workspaceContext?.unique,
			(unique) => {
				// We can't save if we don't have a unique
				if (unique === undefined) {
					this.disable();
				} else {
					// Dangerous, cause this could enable despite a class extension decided to disable it?. [NL]
					this.enable();
				}
			},
			'saveWorkspaceActionUniqueObserver',
		);
	}

	protected _gotWorkspaceContext() {
		// Override in subclass
	}

	override async execute() {
		await this._init;
		return await this._workspaceContext!.requestSubmit();
	}
}

/*
 * @deprecated Use UmbSubmitWorkspaceAction instead
 */
export { UmbSubmitWorkspaceAction as UmbSaveWorkspaceAction };
