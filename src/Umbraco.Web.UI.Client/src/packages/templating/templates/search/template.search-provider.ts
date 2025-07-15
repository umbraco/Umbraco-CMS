import type { UmbTemplateItemModel } from '../repository/item/types.js';
import { UmbTemplateSearchRepository } from './template-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbTemplateSearchItemModel extends UmbTemplateItemModel {
	href: string;
}

export class UmbTemplateSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbTemplateSearchItemModel>
{
	#repository = new UmbTemplateSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbTemplateSearchProvider as api };
