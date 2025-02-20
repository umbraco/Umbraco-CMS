import { UmbDataTypeSearchServerDataSource } from './data-type-search.server.data-source.js';
import type { UmbDataTypeSearchItemModel } from './data-type.search-provider.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDataTypeSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbDataTypeSearchItemModel>, UmbApi
{
	#dataSource: UmbDataTypeSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDataTypeSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
