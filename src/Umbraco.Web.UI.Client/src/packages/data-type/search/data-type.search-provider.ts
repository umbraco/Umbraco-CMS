import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbDataTypeSearchRepository } from './data-type-search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDataTypeSearchProvider extends UmbControllerBase implements UmbSearchProvider {
	#repository = new UmbDataTypeSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}
}

export { UmbDataTypeSearchProvider as api };
