import { UmbWorkspaceContextInterface, UMB_WORKSPACE_CONTEXT } from '../workspace-context/index.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

export interface UmbWorkspaceAction<WorkspaceType = unknown> {
	host: UmbControllerHostElement;
	workspaceContext?: WorkspaceType;
	execute(): Promise<void>;
}

export abstract class UmbWorkspaceActionBase<WorkspaceType extends UmbWorkspaceContextInterface>
	implements UmbWorkspaceAction<WorkspaceType>
{
	host: UmbControllerHostElement;
	workspaceContext?: WorkspaceType;
	constructor(host: UmbControllerHostElement) {
		this.host = host;

		new UmbContextConsumerController(this.host, UMB_WORKSPACE_CONTEXT, (instance) => {
			// TODO: Be aware we are casting here. We should consider a better solution for typing the contexts. (But notice we still want to capture the first workspace...)
			this.workspaceContext = instance as unknown as WorkspaceType;
		});
	}
	abstract execute(): Promise<void>;
}
