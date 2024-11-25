import { UmbMediaSearchServerDataSource } from './media-search.server.data-source.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbMediaSearchItemModel } from './types.js';

export class UmbMediaSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbMediaSearchItemModel>, UmbApi
{
	#dataSource: UmbMediaSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbMediaSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
