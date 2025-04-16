import type { UmbUserGroupItemModel } from './types.js';
import { UMB_USER_GROUP_ITEM_STORE_CONTEXT } from './user-group-item.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbUserGroupItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for User Group items
 */

export class UmbUserGroupItemStore extends UmbItemStoreBase<UmbUserGroupItemModel> {
	/**
	 * Creates an instance of UmbUserGroupItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserGroupItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_GROUP_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbUserGroupItemStore;
