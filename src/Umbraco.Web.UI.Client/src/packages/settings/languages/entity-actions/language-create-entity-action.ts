import { UmbLanguageRepository } from '../repository/language.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbLanguageCreateEntityAction extends UmbEntityActionBase<UmbLanguageRepository> {
	// TODO: Could EntityActions take the manifest instead, for more flexibility?
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	// TODO: Generate the href or retrieve it from something?
	async getHref() {
		return 'section/settings/workspace/language/create';
	}
}
