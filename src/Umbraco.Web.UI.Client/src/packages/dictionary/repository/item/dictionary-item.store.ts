import type { UmbDictionaryItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**

 * @class UmbDictionaryItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Dictionary items
 */

export class UmbDictionaryItemStore extends UmbItemStoreBase<UmbDictionaryItemModel> {
	/**
	 * Creates an instance of UmbDictionaryItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DICTIONARY_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_DICTIONARY_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDictionaryItemStore>('UmbDictionaryItemStore');

export { UmbDictionaryItemStore as api };
