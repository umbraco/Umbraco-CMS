import { UmbTemplateQueryServerDataSource } from './template-query.server.data-source.js';
import type { UmbExecuteTemplateQueryArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbTemplateQueryRepository extends UmbRepositoryBase {
	#querySource: UmbTemplateQueryServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#querySource = new UmbTemplateQueryServerDataSource(this);
	}

	async requestTemplateQuerySettings() {
		return this.#querySource.getTemplateQuerySettings();
	}

	async executeTemplateQuery(args: UmbExecuteTemplateQueryArgs) {
		return this.#querySource.executeTemplateQuery(args);
	}
}
