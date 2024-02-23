import type { UmbUserItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbUserItemStore
 * @extends {UmbItemStoreBase}
 * @description - Data Store for user items
 */

// TODO: add UmbItemStoreInterface when changed to uniques
export class UmbUserItemStore extends UmbItemStoreBase<UmbUserItemModel> {
	/**
	 * Creates an instance of UmbUserItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbUserItemStore;

export const UMB_USER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbUserItemStore>('UmbUserItemStore');
