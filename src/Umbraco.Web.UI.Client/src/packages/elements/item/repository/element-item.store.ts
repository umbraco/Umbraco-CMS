import type { UmbElementDetailModel } from '../../types.js';
import { UMB_ELEMENT_ITEM_STORE_CONTEXT } from './element-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbElementItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Element items
 */

export class UmbElementItemStore extends UmbItemStoreBase<UmbElementDetailModel> {
	/**
	 * Creates an instance of UmbElementItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbElementItemStore as api };
