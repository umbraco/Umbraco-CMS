import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

export interface UmbWorkspaceAction<T> {
	host: UmbControllerHostInterface;
	workspaceContext?: T;
}

export class UmbWorkspaceActionBase<WorkspaceType> implements UmbWorkspaceAction<WorkspaceType> {
	host: UmbControllerHostInterface;
	workspaceContext?: WorkspaceType;
	constructor(host: UmbControllerHostInterface) {
		this.host = host;

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance: WorkspaceType) => {
			this.workspaceContext = instance;
		});
	}
}
