import { UmbDocumentSearchRepository } from './document-search.repository.js';
import type { UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs } from './types.js';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPagedModel, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * The document search provider
 * @class UmbDocumentSearchProvider
 * @augments {UmbControllerBase}
 * @implements {UmbSearchProvider<UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs>}
 */
export class UmbDocumentSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs>
{
	#repository = new UmbDocumentSearchRepository(this);

	/**
	 * Search for documents
	 * @param {UmbDocumentSearchRequestArgs} args - The arguments for the search
	 * @returns {Promise<UmbRepositoryResponse<UmbPagedModel<UmbDocumentSearchItemModel>>>} - The search results
	 * @memberof UmbDocumentSearchProvider
	 */
	search(
		args: UmbDocumentSearchRequestArgs,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbDocumentSearchItemModel>>> {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDocumentSearchProvider as api };
