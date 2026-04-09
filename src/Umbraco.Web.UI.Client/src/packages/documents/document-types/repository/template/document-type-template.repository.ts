import type { UmbDocumentTypeTemplateModel } from '../../types.js';
import { UmbDocumentTypeTemplateServerDataSource } from './document-type-template.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentTypeTemplateRepository extends UmbRepositoryBase {
	#templateSource: UmbDocumentTypeTemplateServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#templateSource = new UmbDocumentTypeTemplateServerDataSource(this);
	}

	async createTemplate(unique: string, model: UmbDocumentTypeTemplateModel): Promise<UmbRepositoryResponse<string>> {
		return await this.#templateSource.createTemplate(unique, model);
	}
}

export { UmbDocumentTypeTemplateRepository as api };
