import { UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT } from './media-type-item.store.context-token.js';
import type { UmbMediaTypeItemModel } from './index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
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
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbMediaTypeItemStore;
