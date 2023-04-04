import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export class UmbCreateFromSnippetPartialViewAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		throw new Error('Method not implemented.');
	}
}
