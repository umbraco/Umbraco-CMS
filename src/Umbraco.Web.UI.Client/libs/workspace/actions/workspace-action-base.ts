import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

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

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance: WorkspaceType) => {
			this.workspaceContext = instance;
		});
	}
}
