import type { UmbUserDetailModel } from '../types.js';
import { UMB_COLLECTION_VIEW_USER_GRID } from './views/index.js';
import type { UmbUserCollectionFilterModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UserOrderModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserCollectionContext extends UmbDefaultCollectionContext<
	UmbUserDetailModel,
	UmbUserCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_VIEW_USER_GRID);
	}

	/**
	 * Sets the state filter for the collection and refreshes the collection.
	 * @param {Array<UserStateModel>} selection
	 * @memberof UmbUserCollectionContext
	 */
	setStateFilter(selection: Array<UserStateModel>) {
		this.setFilter({ userStates: selection });
	}

	/**
	 * Sets the order by filter for the collection and refreshes the collection.
	 * @param {UserOrderModel} orderBy
	 * @memberof UmbUserCollectionContext
	 */
	setOrderByFilter(orderBy: UserOrderModel) {
		this.setFilter({ orderBy });
	}

	/**
	 * Sets the user group filter for the collection and refreshes the collection.
	 * @param {Array<string>} selection
	 * @memberof UmbUserCollectionContext
	 */
	setUserGroupFilter(selection: Array<string>) {
		this.setFilter({ userGroupIds: selection });
	}
}

export default UmbUserCollectionContext;
