import { UmbElementSearchRepository } from './element-search.repository.js';
import type { UmbElementSearchItemModel } from './types.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The element search provider
 * @class UmbElementSearchProvider
 * @augments {UmbControllerBase}
 * @implements {UmbSearchProvider<UmbElementSearchItemModel>}
 */
export class UmbElementSearchProvider extends UmbControllerBase implements UmbSearchProvider<UmbElementSearchItemModel> {
	#repository = new UmbElementSearchRepository(this);

	/**
	 * Search for elements
	 * @param {UmbSearchRequestArgs} args - The arguments for the search
	 * @returns {Promise<UmbRepositoryResponse<UmbPagedModel<UmbElementSearchItemModel>>>} - The search results
	 * @memberof UmbElementSearchProvider
	 */
	search(args: UmbSearchRequestArgs): Promise<UmbRepositoryResponse<UmbPagedModel<UmbElementSearchItemModel>>> {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbElementSearchProvider as api };
