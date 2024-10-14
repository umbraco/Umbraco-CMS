import type { UmbStylesheetItemModel } from '../../types.js';
import { UMB_STYLESHEET_ITEM_STORE_CONTEXT } from './stylesheet-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbStylesheetItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for Stylesheet items
 */

export class UmbStylesheetItemStore extends UmbItemStoreBase<UmbStylesheetItemModel> {
	/**
	 * Creates an instance of UmbStylesheetItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStylesheetItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbStylesheetItemStore;
