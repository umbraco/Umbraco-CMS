import { UmbTemplateQueryServerDataSource } from './template-query.server.data-source.js';
import type { UmbExecuteTemplateQueryRequestModel } from './types.js';
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

	async executeTemplateQuery(args: UmbExecuteTemplateQueryRequestModel) {
		return this.#querySource.executeTemplateQuery(args);
	}
}

export default UmbTemplateQueryRepository;
