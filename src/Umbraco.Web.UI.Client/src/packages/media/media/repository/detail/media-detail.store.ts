import type { UmbMediaDetailModel } from '../../types.js';
import { UMB_MEDIA_DETAIL_STORE_CONTEXT } from './media-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbMediaDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Media Details
 */
export class UmbMediaDetailStore extends UmbDetailStoreBase<UmbMediaDetailModel> {
	/**
	 * Creates an instance of UmbMediaDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbMediaDetailStore;
