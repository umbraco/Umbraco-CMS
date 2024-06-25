import type { UmbMemberTypeItemModel } from '../repository/item/types.js';
import { UmbMemberTypeSearchRepository } from './member-type-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbMemberTypeSearchItemModel extends UmbMemberTypeItemModel {
	href: string;
}

export class UmbMemberTypeSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbMemberTypeSearchItemModel>
{
	#repository = new UmbMemberTypeSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMemberTypeSearchProvider as api };
