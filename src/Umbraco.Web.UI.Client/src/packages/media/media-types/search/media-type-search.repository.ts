import { UmbMediaTypeSearchServerDataSource } from './media-type-search.server.data-source.js';
import type { UmbMediaTypeSearchItemModel } from './media-type.search-provider.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTypeSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbMediaTypeSearchItemModel>, UmbApi
{
	#dataSource: UmbMediaTypeSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbMediaTypeSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
