import { UmbDocumentSearchRepository } from './document-search.repository.js';
import type { UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs } from './types.js';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs>
{
	#repository = new UmbDocumentSearchRepository(this);

	search(args: UmbDocumentSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDocumentSearchProvider as api };
