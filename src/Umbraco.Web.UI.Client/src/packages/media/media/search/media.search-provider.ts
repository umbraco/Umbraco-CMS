import { UmbMediaSearchRepository } from './media-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbMediaSearchItemModel } from './types.js';

export class UmbMediaSearchProvider extends UmbControllerBase implements UmbSearchProvider<UmbMediaSearchItemModel> {
	#repository = new UmbMediaSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMediaSearchProvider as api };
