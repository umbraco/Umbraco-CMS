import type { UmbMediaDetailModel } from '../../types.js';
import { UMB_MEDIA_ITEM_STORE_CONTEXT } from './media-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbMediaItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Media items
 */

export class UmbMediaItemStore extends UmbItemStoreBase<UmbMediaDetailModel> {
	/**
	 * Creates an instance of UmbMediaItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbMediaItemStore;
