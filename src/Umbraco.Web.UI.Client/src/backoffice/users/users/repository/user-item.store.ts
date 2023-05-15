import { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbUserItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for user items
 */

export class UmbUserItemStore
	extends UmbStoreBase<UserItemResponseModel>
	implements UmbItemStore<UserItemResponseModel>
{
	/**
	 * Creates an instance of UmbUserItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_USER_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<UserItemResponseModel>([], (x) => x.id)
		);
	}

	items(ids: Array<string>) {
		return this._data.getObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_USER_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserItemStore>('UmbUserItemStore');
