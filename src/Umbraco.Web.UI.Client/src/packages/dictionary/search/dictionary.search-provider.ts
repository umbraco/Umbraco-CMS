import type { UmbDictionaryItemModel } from '../index.js';
import { UmbDictionarySearchRepository } from './dictionary-search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export interface UmbDictionarySearchItemModel extends UmbDictionaryItemModel {
	href: string;
}

export class UmbDictionarySearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDictionarySearchItemModel>
{
	#repository = new UmbDictionarySearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDictionarySearchProvider as api };
