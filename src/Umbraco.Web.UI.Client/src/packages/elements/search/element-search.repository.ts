import { UmbElementSearchServerDataSource } from './element-search.server.data-source.js';
import type { UmbElementSearchItemModel } from './types.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbElementSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbElementSearchItemModel>, UmbApi
{
	#dataSource: UmbElementSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbElementSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
