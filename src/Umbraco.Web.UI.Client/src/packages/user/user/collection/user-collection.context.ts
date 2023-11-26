import { UMB_USER_ENTITY_TYPE, UmbUserCollectionFilterModel, UmbUserDetail } from '../types.js';
import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from './repository/manifests.js';
import { UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UserOrderModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserCollectionContext extends UmbCollectionContext<UmbUserDetail, UmbUserCollectionFilterModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_ENTITY_TYPE, UMB_USER_COLLECTION_REPOSITORY_ALIAS, { pageSize: 50 });
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
