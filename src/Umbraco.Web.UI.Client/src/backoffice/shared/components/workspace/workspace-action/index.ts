import { UmbEntityActionBase } from '../../entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { UmbRepositoryFactory } from '@umbraco-cms/models';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

export class UmbWorkspaceAction<T> extends UmbEntityActionBase<T> {
	workspaceContext: any;
	constructor(host: UmbControllerHostInterface, repository: UmbRepositoryFactory<T>, unique: string) {
		super(host, repository, unique);

		new UmbContextConsumerController(this.host, 'umbWorkspaceContext', (instance) => {
			this.workspaceContext = instance;
		});
	}
}
