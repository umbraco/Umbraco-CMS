import type { UmbMemberDetailModel } from '../types.js';
import { UMB_MEMBER_COLLECTION_MEMBER_TYPE_FACET_FILTER_ALIAS } from './filter/member-type/constants.js';
import { UMB_MEMBER_COLLECTION_MEMBER_GROUP_FACET_FILTER_ALIAS } from './filter/member-group/constants.js';
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

		const memberTypeFilters = activeFilters.filter((f) => f.alias === UMB_MEMBER_COLLECTION_MEMBER_TYPE_FACET_FILTER_ALIAS);
		if (memberTypeFilters.length) args.memberTypeId = memberTypeFilters.map((f) => f.unique);

		const memberGroupFilters = activeFilters.filter((f) => f.alias === UMB_MEMBER_COLLECTION_MEMBER_GROUP_FACET_FILTER_ALIAS);
		if (memberGroupFilters.length) args.memberGroupName = memberGroupFilters.map((f) => f.unique);

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
