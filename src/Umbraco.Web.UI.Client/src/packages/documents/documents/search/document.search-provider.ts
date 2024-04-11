import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbDocumentSearchRepository } from './document-search.repository.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentSearchProvider extends UmbControllerBase implements UmbApi {
	#repository = new UmbDocumentSearchRepository(this);

	search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}
}

export { UmbDocumentSearchProvider as api };
