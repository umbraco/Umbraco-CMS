import { UmbDocumentSearchRepository } from './document-search.repository.js';
import type { UmbDocumentSearchItemModel } from './types.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDocumentSearchItemModel>
{
	#repository = new UmbDocumentSearchRepository(this);

	search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDocumentSearchProvider as api };
