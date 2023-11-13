import { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDataTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Data Type items
 */

export class UmbDataTypeItemStore extends UmbEntityItemStore<DataTypeItemResponseModel> {
	/**
	 * Creates an instance of UmbDataTypeItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_DATA_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDataTypeItemStore>('UmbDataTypeItemStore');
