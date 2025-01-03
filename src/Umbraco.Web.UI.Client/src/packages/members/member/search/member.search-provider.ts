import type { UmbMemberItemModel } from '../repository/item/types.js';
import { UmbMemberSearchRepository } from './member-search.repository.js';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbMemberSearchItemModel extends UmbMemberItemModel {
	href: string;
}

export class UmbMemberSearchProvider extends UmbControllerBase implements UmbSearchProvider<UmbMemberSearchItemModel> {
	#repository = new UmbMemberSearchRepository(this);

	async search(args: UmbSearchRequestArgs) {
		return this.#repository.search(args);
	}

	override destroy(): void {
		this.#repository.destroy();
	}
}

export { UmbMemberSearchProvider as api };
