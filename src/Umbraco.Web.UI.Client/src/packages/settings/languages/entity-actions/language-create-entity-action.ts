import { UmbLanguageRepository } from '../repository/language.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbLanguageCreateEntityAction extends UmbEntityActionBase<UmbLanguageRepository> {
	// TODO: Could EntityActions take the manifest instead, for more flexibility?
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);
	}

	async execute() {
		// TODO: Generate the href or retrieve it from something?
		history.pushState(null, '', `section/settings/workspace/language/create`);
	}
}
