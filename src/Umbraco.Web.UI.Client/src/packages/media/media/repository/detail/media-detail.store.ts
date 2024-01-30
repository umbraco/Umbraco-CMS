import type { UmbMediaDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbMediaDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Media Details
 */
export class UmbMediaDetailStore extends UmbDetailStoreBase<UmbMediaDetailModel> {
	/**
	 * Creates an instance of UmbMediaDetailStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMediaDetailStore>('UmbMediaDetailStore');
