import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore } from '@umbraco-cms/backoffice/store';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @export
 * @class UmbUserGroupItemStore
 * @extends {UmbEntityItemStore}
 * @description - Data Store for user group items
 */

export class UmbUserGroupItemStore extends UmbEntityItemStore<UserGroupItemResponseModel> {
	/**
	 * Creates an instance of UmbUserGroupItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserGroupItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_GROUP_ITEM_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_USER_GROUP_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserGroupItemStore>(
	'UmbUserGroupItemStore',
);
