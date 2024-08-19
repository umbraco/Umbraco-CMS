import type { UmbUserItemModel } from './types.js';
import { UMB_USER_ITEM_STORE_CONTEXT } from './user-item.store.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbUserItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for user items
 */

// TODO: add UmbItemStoreInterface when changed to uniques
export class UmbUserItemStore extends UmbItemStoreBase<UmbUserItemModel> {
	/**
	 * Creates an instance of UmbUserItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbUserItemStore;
