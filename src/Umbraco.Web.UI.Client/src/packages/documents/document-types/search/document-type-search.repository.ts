import { UmbDocumentTypeSearchServerDataSource } from './document-type-search.server.data-source.js';
import type { UmbDocumentTypeSearchItemModel } from './document-type.search-provider.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentTypeSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbDocumentTypeSearchItemModel>, UmbApi
{
	#dataSource: UmbDocumentTypeSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDocumentTypeSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
