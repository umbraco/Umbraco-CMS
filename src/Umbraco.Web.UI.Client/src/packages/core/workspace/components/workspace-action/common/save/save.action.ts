import type { MetaWorkspaceAction } from '../../../../types.js';
import { UMB_SAVEABLE_WORKSPACE_CONTEXT } from '../../../../contexts/tokens/index.js';
import type { UmbSaveableWorkspaceContext } from '../../../../contexts/tokens/index.js';
import { UmbWorkspaceActionBase } from '../../workspace-action-base.controller.js';
import type { UmbSaveWorkspaceActionArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSaveWorkspaceAction<
	ArgsMetaType extends MetaWorkspaceAction = MetaWorkspaceAction,
	WorkspaceContextType extends UmbSaveableWorkspaceContext = UmbSaveableWorkspaceContext,
> extends UmbWorkspaceActionBase<ArgsMetaType> {
	protected _retrieveWorkspaceContext: Promise<unknown>;
	protected _workspaceContext?: WorkspaceContextType;

	constructor(host: UmbControllerHost, args: UmbSaveWorkspaceActionArgs<ArgsMetaType, WorkspaceContextType>) {
		super(host, args);

		this._retrieveWorkspaceContext = this.consumeContext(
			args.workspaceContextToken ?? UMB_SAVEABLE_WORKSPACE_CONTEXT,
			(context) => {
				this._workspaceContext = context as WorkspaceContextType | undefined;
				this.#observeUnique();
				this._gotWorkspaceContext();
			},
		).asPromise();
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
		await this._retrieveWorkspaceContext;
		return await this._workspaceContext?.requestSave();
	}
}
