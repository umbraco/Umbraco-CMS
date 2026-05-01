import { UmbElementSearchRepository } from './element-search.repository.js';
import type { UmbElementSearchItemModel } from './types.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbElementSearchProvider extends UmbControllerBase implements UmbSearchProvider<UmbElementSearchItemModel> {
	#repository = new UmbElementSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbElementSearchProvider as api };
