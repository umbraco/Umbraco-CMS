import type { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_DETAIL_STORE_CONTEXT } from './stylesheet-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbStylesheetDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for stylesheet detail
 */
export class UmbStylesheetDetailStore extends UmbDetailStoreBase<UmbStylesheetDetailModel> {
	/**
	 * Creates an instance of UmbStylesheetDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbStylesheetDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbStylesheetDetailStore;
