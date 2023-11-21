import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateScriptAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		if (this.unique !== null) {
			// Note: %2F is a slash (/)
			this.unique = this.unique.replace(/\//g, '%2F');
		}

		history.pushState(null, '', `section/settings/workspace/script/create/${this.unique ?? 'null'}`);
	}
}
