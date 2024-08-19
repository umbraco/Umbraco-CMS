import type { UmbMediaTypeDetailModel } from '../../types.js';
import { UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT } from './media-type-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbMediaTypeStore
 * @augments {UmbDetailStoreBase}
 * @description - Data Store for Media Types
 */
export class UmbMediaTypeDetailStore extends UmbDetailStoreBase<UmbMediaTypeDetailModel> {
	/**
	 * Creates an instance of UmbMediaTypeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaTypeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbMediaTypeDetailStore;
