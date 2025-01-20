import { UmbDocumentSearchServerDataSource } from './document-search.server.data-source.js';
import type { UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs } from './types.js';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbDocumentSearchItemModel>, UmbApi
{
	#dataSource: UmbDocumentSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#dataSource = new UmbDocumentSearchServerDataSource(this);
	}

	/**
	 * Search for documents
	 * @param {UmbDocumentSearchRequestArgs} args - The arguments for the search
	 * @returns {Promise<UmbRepositoryResponse<UmbPagedModel<UmbDocumentSearchItemModel>>>} - The search results
	 * @memberof UmbDocumentSearchRepository
	 */
	search(
		args: UmbDocumentSearchRequestArgs,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbDocumentSearchItemModel>>> {
		return this.#dataSource.search(args);
	}
}
