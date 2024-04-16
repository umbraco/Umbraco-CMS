import { UmbDocumentSearchRepository } from './document-search.repository.js';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentSearchProvider extends UmbControllerBase implements UmbApi {
	#repository = new UmbDocumentSearchRepository(this);

	search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDocumentSearchProvider as api };
