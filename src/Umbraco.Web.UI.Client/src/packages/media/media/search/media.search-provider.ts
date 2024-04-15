import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbMediaItemModel } from '../index.js';
import { UmbMediaSearchRepository } from './media-search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbMediaSearchItemModel extends UmbMediaItemModel {}

export class UmbMediaSearchProvider extends UmbControllerBase implements UmbSearchProvider<UmbMediaSearchItemModel> {
	#repository = new UmbMediaSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMediaSearchProvider as api };
