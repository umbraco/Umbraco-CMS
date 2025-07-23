import { UmbMediaSearchRepository } from './media-search.repository.js';
import type { UmbMediaSearchItemModel, UmbMediaSearchRequestArgs } from './types.js';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMediaSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbMediaSearchItemModel, UmbMediaSearchRequestArgs>
{
	#repository = new UmbMediaSearchRepository(this);

	async search(args: UmbMediaSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMediaSearchProvider as api };
