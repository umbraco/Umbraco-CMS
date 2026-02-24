import { UMB_EXTENSION_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbExtensionItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbExtensionItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Extension items
 */
export class UmbExtensionItemStore extends UmbItemStoreBase<UmbExtensionItemModel> {
	/**
	 * Creates an instance of UmbExtensionItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbExtensionItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_EXTENSION_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbExtensionItemStore as api };
