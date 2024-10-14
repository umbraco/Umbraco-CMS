import type { UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT } from './partial-view-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbPartialViewDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Partial View detail
 */
export class UmbPartialViewDetailStore extends UmbDetailStoreBase<UmbPartialViewDetailModel> {
	/**
	 * Creates an instance of UmbPartialViewDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbPartialViewDetailStore;
