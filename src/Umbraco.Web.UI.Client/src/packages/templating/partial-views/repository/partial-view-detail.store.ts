import { UmbPartialViewDetailModel } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbPartialViewDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Partial View detail
 */
export class UmbPartialViewDetailStore extends UmbDetailStoreBase<UmbPartialViewDetailModel> {
	/**
	 * Creates an instance of UmbPartialViewDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_PARTIAL_VIEW_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbPartialViewDetailStore>(
	'UmbPartialViewDetailStore',
);
