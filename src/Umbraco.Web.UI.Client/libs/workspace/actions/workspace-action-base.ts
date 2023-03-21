import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

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

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance: WorkspaceType) => {
			this.workspaceContext = instance;
		});
	}
}
