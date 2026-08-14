import type { UmbElementDetailModel } from '../../types.js';
import { UMB_ELEMENT_DETAIL_STORE_CONTEXT } from './element-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbElementDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Element Details
 */
export class UmbElementDetailStore extends UmbDetailStoreBase<UmbElementDetailModel> {
	/**
	 * Creates an instance of UmbElementDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_DETAIL_STORE_CONTEXT.toString());
	}
}

export { UmbElementDetailStore as api };
