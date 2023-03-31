import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController, UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

export interface UmbWorkspaceAction<T = unknown> {
	host: UmbControllerHostElement;
	workspaceContext?: T;
	execute(): Promise<void>;
}

export class UmbWorkspaceActionBase<WorkspaceType> {
	host: UmbControllerHostElement;
	workspaceContext?: WorkspaceType;
	constructor(host: UmbControllerHostElement) {
		this.host = host;

		new UmbContextConsumerController(this.host, UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.workspaceContext = instance as WorkspaceType;
		});
	}
}
