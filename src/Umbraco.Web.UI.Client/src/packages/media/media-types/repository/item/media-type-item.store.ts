import type { UmbMediaTypeItemModel } from './index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMediaTypeItemStore
 * @extends {UmbItemStoreBase}
 * @description - Data Store for Media Type items
 */

export class UmbMediaTypeItemStore extends UmbItemStoreBase<UmbMediaTypeItemModel> {
	/**
	 * Creates an instance of UmbMediaTypeItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeItemStore>('UmbMediaTypeItemStore');
