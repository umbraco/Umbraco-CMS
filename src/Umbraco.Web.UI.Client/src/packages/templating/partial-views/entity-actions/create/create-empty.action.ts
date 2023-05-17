import { UmbEntityActionBase } from '@umbraco-cms/backoffice/components';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateEmptyPartialViewAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		throw new Error('Method not implemented.');
	}
}
