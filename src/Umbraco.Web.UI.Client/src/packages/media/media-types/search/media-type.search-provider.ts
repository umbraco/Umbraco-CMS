import type { UmbMediaTypeItemModel } from '../index.js';
import { UmbMediaTypeSearchRepository } from './media-type-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbMediaTypeSearchItemModel extends UmbMediaTypeItemModel {
	href: string;
}

export class UmbMediaTypeSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbMediaTypeSearchItemModel>
{
	#repository = new UmbMediaTypeSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMediaTypeSearchProvider as api };
