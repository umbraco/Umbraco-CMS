import { UmbDictionarySearchServerDataSource } from './dictionary-search.server.data-source.js';
import type { UmbDictionarySearchItemModel } from './dictionary.search-provider.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export class UmbDictionarySearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbDictionarySearchItemModel>, UmbApi
{
	#dataSource: UmbDictionarySearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDictionarySearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
