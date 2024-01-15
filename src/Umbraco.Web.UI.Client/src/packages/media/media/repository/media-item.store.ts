import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore } from '@umbraco-cms/backoffice/store';
import { MediaItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @export
 * @class UmbMediaItemStore
 * @extends {UmbEntityItemStore}
 * @description - Data Store for Media items
 */

export class UmbMediaItemStore extends UmbEntityItemStore<MediaItemResponseModel> {
	/**
	 * Creates an instance of UmbMediaItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMediaItemStore>('UmbMediaItemStore');
