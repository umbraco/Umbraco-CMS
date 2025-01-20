import { UmbDocumentSearchServerDataSource } from './document-search.server.data-source.js';
import type { UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs } from './types.js';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbDocumentSearchItemModel>, UmbApi
{
	#dataSource: UmbDocumentSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDocumentSearchServerDataSource(this);
	}

	search(args: UmbDocumentSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
