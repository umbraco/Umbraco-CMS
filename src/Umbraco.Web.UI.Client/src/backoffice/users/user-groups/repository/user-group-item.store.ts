import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @export
 * @class UmbUserGroupItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for user group items
 */

export class UmbUserGroupItemStore
	extends UmbStoreBase<UserGroupItemResponseModel>
	implements UmbItemStore<UserGroupItemResponseModel>
{
	/**
	 * Creates an instance of UmbUserGroupItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserGroupItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_USER_GROUP_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<UserGroupItemResponseModel>([], (x) => x.id)
		);
	}

	items(ids: Array<string>) {
		return this._data.getObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_USER_GROUP_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserGroupItemStore>(
	'UmbUserGroupItemStore'
);
