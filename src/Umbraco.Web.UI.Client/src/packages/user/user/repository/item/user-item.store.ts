import type { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbUserItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for user items
 */

// TODO: add UmbItemStoreInterface when changed to uniques
export class UmbUserItemStore extends UmbStoreBase<UserItemResponseModel> {
	/**
	 * Creates an instance of UmbUserItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_ITEM_STORE_CONTEXT.toString(), new UmbArrayState<UserItemResponseModel>([], (x) => x.id));
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_USER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbUserItemStore>('UmbUserItemStore');
