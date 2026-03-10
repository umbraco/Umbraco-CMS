import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UmbPropertyEditorDataSourceSearchRepository } from './search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export class UmbPropertyEditorDataSourceSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbPropertyEditorDataSourceItemModel>
{
	#repository = new UmbPropertyEditorDataSourceSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbPropertyEditorDataSourceSearchProvider as api };
