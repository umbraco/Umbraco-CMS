import type { UmbLanguageItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbLanguageItemStore
 * @extends {UmbStoreBase}
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

export const UMB_LANGUAGE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbLanguageItemStore>('UmbLanguageItemStore');
