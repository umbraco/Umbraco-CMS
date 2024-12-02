import type { UmbDataTypeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbDataTypeItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Data Type items
 */

export class UmbDataTypeItemStore extends UmbItemStoreBase<UmbDataTypeItemModel> {
	/**
	 * Creates an instance of UmbDataTypeItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_DATA_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDataTypeItemStore>('UmbDataTypeItemStore');

export { UmbDataTypeItemStore as api };
