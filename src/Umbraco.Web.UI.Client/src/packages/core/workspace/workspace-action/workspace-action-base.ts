import { UmbWorkspaceContextInterface, UMB_WORKSPACE_CONTEXT } from '../workspace-context/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbWorkspaceAction<WorkspaceType = unknown> extends UmbApi {
	host: UmbControllerHost;
	workspaceContext?: WorkspaceType;
	execute(): Promise<void>;
}

export abstract class UmbWorkspaceActionBase<WorkspaceContextType extends UmbWorkspaceContextInterface>
	implements UmbWorkspaceAction<WorkspaceContextType>
{
	host: UmbControllerHost;
	workspaceContext?: WorkspaceContextType;
	constructor(host: UmbControllerHost) {
		this.host = host;

		new UmbContextConsumerController(this.host, UMB_WORKSPACE_CONTEXT, (instance) => {
			// TODO: Be aware we are casting here. We should consider a better solution for typing the contexts. (But notice we still want to capture the first workspace...)
			this.workspaceContext = instance as unknown as WorkspaceContextType;
		});
	}
	abstract execute(): Promise<void>;
}
