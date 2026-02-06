import type { UmbExtensionItemModel } from '../item/types.js';
import { UmbExtensionSearchRepository } from './search.repository.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export class UmbExtensionSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbExtensionItemModel>
{
	#repository = new UmbExtensionSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbExtensionSearchProvider as api };
