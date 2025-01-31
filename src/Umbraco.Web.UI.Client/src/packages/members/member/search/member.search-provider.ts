import { UmbMemberSearchRepository } from './member-search.repository.js';
import type { UmbMemberSearchItemModel, UmbMemberSearchRequestArgs } from './types.js';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMemberSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbMemberSearchItemModel, UmbMemberSearchRequestArgs>
{
	#repository = new UmbMemberSearchRepository(this);

	async search(args: UmbMemberSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMemberSearchProvider as api };
