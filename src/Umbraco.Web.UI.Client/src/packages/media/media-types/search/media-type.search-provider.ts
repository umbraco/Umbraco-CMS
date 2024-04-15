import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbMediaTypeItemModel } from '../index.js';
import { UmbMediaTypeSearchRepository } from './media-type-search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbMediaTypeSearchItemModel extends UmbMediaTypeItemModel {}

export class UmbMediaTypeSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbMediaTypeSearchItemModel>
{
	#repository = new UmbMediaTypeSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMediaTypeSearchProvider as api };
