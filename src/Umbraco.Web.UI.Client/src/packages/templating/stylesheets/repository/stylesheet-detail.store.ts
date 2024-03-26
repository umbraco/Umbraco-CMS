import type { UmbStylesheetDetailModel } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbStylesheetDetailStore
 * @extends {UmbStoreBase}
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

export const UMB_STYLESHEET_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbStylesheetDetailStore>(
	'UmbStylesheetDetailStore',
);
