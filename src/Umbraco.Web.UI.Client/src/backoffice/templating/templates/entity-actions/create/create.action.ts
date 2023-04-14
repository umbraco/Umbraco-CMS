import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbCreateEntityAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	// TODO: can we make this a generic create action
	async execute() {
		// TODO: get entity type from repository?
		const url = `section/settings/template/create/${this.unique || 'root'}`;
		// TODO: how do we handle this with a href?
		history.pushState(null, '', url);
	}
}
