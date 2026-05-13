import { UmbElementSearchServerDataSource } from './element-search.server.data-source.js';
import type { UmbElementSearchItemModel } from './types.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The element search repository
 * @class UmbElementSearchRepository
 * @augments {UmbControllerBase}
 * @implements {UmbSearchRepository<UmbElementSearchItemModel>}
 */
export class UmbElementSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbElementSearchItemModel>, UmbApi
{
	#dataSource: UmbElementSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbElementSearchServerDataSource(this);
	}

	/**
	 * Search for elements
	 * @param {UmbSearchRequestArgs} args - The arguments for the search
	 * @returns {Promise<UmbRepositoryResponse<UmbPagedModel<UmbElementSearchItemModel>>>} - The search results
	 * @memberof UmbElementSearchRepository
	 */
	search(args: UmbSearchRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<UmbElementSearchItemModel>>> {
		return this.#dataSource.search(args);
	}
}
