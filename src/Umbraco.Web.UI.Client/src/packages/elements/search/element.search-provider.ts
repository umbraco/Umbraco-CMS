import { UmbElementSearchRepository } from './element-search.repository.js';
import type { UmbElementSearchItemModel, UmbElementSearchRequestArgs } from './types.js';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The element search provider
 * @class UmbElementSearchProvider
 * @augments {UmbControllerBase}
 * @implements {UmbSearchProvider<UmbElementSearchItemModel, UmbElementSearchRequestArgs>}
 */
export class UmbElementSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbElementSearchItemModel, UmbElementSearchRequestArgs>
{
	#repository = new UmbElementSearchRepository(this);

	/**
	 * Search for elements
	 * @param {UmbElementSearchRequestArgs} args - The arguments for the search
	 * @returns {Promise<UmbRepositoryResponse<UmbPagedModel<UmbElementSearchItemModel>>>} - The search results
	 * @memberof UmbElementSearchProvider
	 */
	search(args: UmbElementSearchRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<UmbElementSearchItemModel>>> {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbElementSearchProvider as api };
