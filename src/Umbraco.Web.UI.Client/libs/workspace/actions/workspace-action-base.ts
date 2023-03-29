import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController, UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

export interface UmbWorkspaceAction<T = unknown> {
	host: UmbControllerHostInterface;
	workspaceContext?: T;
	execute(): Promise<void>;
}

export class UmbWorkspaceActionBase<WorkspaceType> {
	host: UmbControllerHostInterface;
	workspaceContext?: WorkspaceType;
	constructor(host: UmbControllerHostInterface) {
		this.host = host;

		new UmbContextConsumerController(this.host, UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.workspaceContext = instance as WorkspaceType;
		});
	}
}
