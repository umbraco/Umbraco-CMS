import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

export class UmbWorkspaceAction<WorkspaceType> {
	host: UmbControllerHostInterface;
	workspaceContext?: WorkspaceType;
	constructor(host: UmbControllerHostInterface) {
		this.host = host;

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance: WorkspaceType) => {
			this.workspaceContext = instance;
		});
	}
}
