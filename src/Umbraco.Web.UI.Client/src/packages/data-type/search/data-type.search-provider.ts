import type { UmbDataTypeItemModel } from '../index.js';
import { UmbDataTypeSearchRepository } from './data-type-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbDataTypeSearchItemModel extends UmbDataTypeItemModel {
	href: string;
}

export class UmbDataTypeSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbDataTypeSearchItemModel>
{
	#repository = new UmbDataTypeSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbDataTypeSearchProvider as api };
