import { UmbEntityActionBase } from '../../entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

export class UmbWorkspaceAction<T> extends UmbEntityActionBase<T> {
	workspaceContext: any;
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance) => {
			this.workspaceContext = instance;
		});
	}
}
