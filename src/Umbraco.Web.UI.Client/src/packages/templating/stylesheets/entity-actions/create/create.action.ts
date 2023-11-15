import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateStylesheetAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		if (this.unique !== null) {
			// Note: %2F is a slash (/)
			this.unique = this.unique.replace(/\//g, '%2F');
		}

		history.pushState(null, '', `section/settings/workspace/stylesheet/create/${this.unique ?? 'null'}/view/code`);
	}
}
