import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateEmptyPartialViewAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		history.pushState(null, '', `section/settings/workspace/partial-view/create/${this.unique ?? 'null'}/`);
	}
}
