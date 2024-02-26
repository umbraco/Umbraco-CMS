import type { UmbDictionaryDetailRepository } from '../../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export default class UmbCreateDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryDetailRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);
	}

	async execute() {
		history.pushState({}, '', `/section/dictionary/workspace/dictionary/create/${this.unique ?? 'null'}`);
	}
}
