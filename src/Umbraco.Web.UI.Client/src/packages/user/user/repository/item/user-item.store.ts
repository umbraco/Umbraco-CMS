import type { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbUserItemStore
 * @extends {UmbEntityItemStore}
 * @description - Data Store for user items
 */

// TODO: add UmbItemStoreInterface when changed to uniques
export class UmbUserItemStore extends UmbEntityItemStore<UserItemResponseModel> {
	/**
	 * Creates an instance of UmbUserItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_USER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbUserItemStore>('UmbUserItemStore');
