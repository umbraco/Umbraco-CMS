import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbPartialViewsRepository } from '../../repository/index.js';

export class UmbCreateFromSnippetPartialViewAction<
	T extends { copy(): Promise<void> }
> extends UmbEntityActionBase<UmbPartialViewsRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		const snippets = await this.repository?.getSnippets({});
		console.log(snippets);
	}
}
