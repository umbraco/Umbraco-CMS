import type { UmbMediaDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMediaItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Media items
 */

export class UmbMediaItemStore extends UmbItemStoreBase<UmbMediaDetailModel> {
	/**
	 * Creates an instance of UmbMediaItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMediaItemStore>('UmbMediaItemStore');
