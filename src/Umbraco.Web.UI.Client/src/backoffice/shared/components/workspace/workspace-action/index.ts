import { UmbActionBase } from '../../../entity-actions';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

export class UmbWorkspaceAction<RepositoryType, WorkspaceType> extends UmbActionBase<RepositoryType> {
	workspaceContext?: WorkspaceType;
	constructor(host: UmbControllerHostInterface, repositoryAlias: string) {
		super(host, repositoryAlias);

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance: WorkspaceType) => {
			this.workspaceContext = instance;
		});
	}
}
