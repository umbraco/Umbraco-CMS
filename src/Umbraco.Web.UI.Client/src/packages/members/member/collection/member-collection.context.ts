import type { UmbMemberDetailModel } from '../types.js';
import type { UmbMemberCollectionFilterModel } from './types.js';
import { UMB_MEMBER_TABLE_COLLECTION_VIEW_ALIAS } from './views/manifests.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberCollectionContext extends UmbDefaultCollectionContext<
	UmbMemberDetailModel,
	UmbMemberCollectionFilterModel
> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TABLE_COLLECTION_VIEW_ALIAS);
	}

	protected override async _getFilterArgs(): Promise<Record<string, any>> {
		const activeFilters = await this.filtering.getActiveFilters();
		const args: Record<string, any> = {};
		for (const filter of activeFilters) {
			if (filter.alias === 'Umb.CollectionFacetFilter.MemberType') {
				args.memberTypeId = filter.value.map((v: { unique: string }) => v.unique);
			}
			if (filter.alias === 'Umb.CollectionFacetFilter.MemberGroup') {
				args.memberGroupName = filter.value.map((v: { unique: string }) => v.unique);
			}
		}
		return args;
	}

	/**
	 * Sets the member type filter for the collection and refreshes the collection.
	 * @param {Array<string>} selection
	 * @memberof UmbMemberCollectionContext
	 */
	setMemberTypeFilter(selection: string) {
		this.setFilter({ memberTypeId: selection });
	}
}

export { UmbMemberCollectionContext as api };
