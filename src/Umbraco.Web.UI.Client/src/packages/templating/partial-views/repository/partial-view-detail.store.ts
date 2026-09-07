import type { UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from './partial-view-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbPartialViewDetailStore
 * @augments {UmbDetailStoreBase}
 * @description - Data Store for Partial View detail
 */
export class UmbPartialViewDetailStore extends UmbDetailStoreBase<UmbPartialViewDetailModel> {
	/**
	 * Creates an instance of UmbPartialViewDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbPartialViewDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT);
	}
}

export default UmbPartialViewDetailStore;
