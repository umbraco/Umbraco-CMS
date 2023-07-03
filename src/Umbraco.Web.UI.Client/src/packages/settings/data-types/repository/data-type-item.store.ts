import { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbDataTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Data Type items
 */

export class UmbDataTypeItemStore
	extends UmbStoreBase<DataTypeItemResponseModel>
	implements UmbItemStore<DataTypeItemResponseModel>
{
	/**
	 * Creates an instance of UmbDataTypeItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DATA_TYPE_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<DataTypeItemResponseModel>([], (x) => x.id)
		);
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_DATA_TYPE_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDataTypeItemStore>('UmbDataTypeItemStore');
