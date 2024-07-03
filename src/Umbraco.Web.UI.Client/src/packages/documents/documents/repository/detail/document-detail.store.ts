import type { UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_DETAIL_STORE_CONTEXT } from './document-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Details
 */
export class UmbDocumentDetailStore extends UmbDetailStoreBase<UmbDocumentDetailModel> {
	/**
	 * Creates an instance of UmbDocumentDetailStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_DETAIL_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentDetailStore as api };
