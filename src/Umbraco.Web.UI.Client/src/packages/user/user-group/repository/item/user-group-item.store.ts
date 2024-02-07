import type { UmbUserGroupItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbUserGroupItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for User Group items
 */

export class UmbUserGroupItemStore extends UmbItemStoreBase<UmbUserGroupItemModel> {
	/**
	 * Creates an instance of UmbUserGroupItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserGroupItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_GROUP_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_USER_GROUP_ITEM_STORE_CONTEXT = new UmbContextToken<UmbUserGroupItemStore>('UmbUserGroupItemStore');
