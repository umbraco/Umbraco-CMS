import type { UmbLanguageItemModel } from './types.js';
import { UMB_LANGUAGE_ITEM_STORE_CONTEXT } from './language-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbLanguageItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Language items
 */

export class UmbLanguageItemStore extends UmbItemStoreBase<UmbLanguageItemModel> {
	/**
	 * Creates an instance of UmbLanguageItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbLanguageItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_LANGUAGE_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbLanguageItemStore;
