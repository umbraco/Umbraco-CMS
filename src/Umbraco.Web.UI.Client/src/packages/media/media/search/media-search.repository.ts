import { UmbMediaSearchServerDataSource } from './media-search.server.data-source.js';
import type { UmbMediaSearchItemModel, UmbMediaSearchRequestArgs } from './types.js';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbMediaSearchItemModel, UmbMediaSearchRequestArgs>, UmbApi
{
	#dataSource: UmbMediaSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbMediaSearchServerDataSource(this);
	}

	search(args: UmbMediaSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
