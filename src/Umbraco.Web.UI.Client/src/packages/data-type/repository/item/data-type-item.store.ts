import type { UmbDataTypeItemModel } from './types.js';
import { UMB_DATA_TYPE_ITEM_STORE_CONTEXT } from './data-type-item.store.context-token.js';
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

export { UmbDataTypeItemStore as api };
