import type { UmbStylesheetItemModel } from '../../types.js';
import { UMB_STYLESHEET_ITEM_STORE_CONTEXT } from './stylesheet-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbStylesheetItemStore
 * @extends {UmbItemStoreBase}
 * @description - Data Store for Stylesheet items
 */

export class UmbStylesheetItemStore extends UmbItemStoreBase<UmbStylesheetItemModel> {
	/**
	 * Creates an instance of UmbStylesheetItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbStylesheetItemStore;
