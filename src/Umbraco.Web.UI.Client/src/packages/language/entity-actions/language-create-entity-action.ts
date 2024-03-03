import type { UmbLanguageDetailRepository } from '../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbLanguageCreateEntityAction extends UmbEntityActionBase<UmbLanguageDetailRepository> {
	// TODO: Could EntityActions take the manifest instead, for more flexibility?
	constructor(host: UmbControllerHost, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);
	}

	async execute() {
		// TODO: Generate the href or retrieve it from something?
		history.pushState(null, '', `section/settings/workspace/language/create`);
	}
}
