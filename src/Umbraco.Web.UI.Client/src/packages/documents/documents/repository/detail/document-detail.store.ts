import { UmbDocumentDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Details
 */
export class UmbDocumentDetailStore extends UmbDetailStoreBase<UmbDocumentDetailModel> {
	/**
	 * Creates an instance of UmbDocumentDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_DOCUMENT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentDetailStore>('UmbDocumentDetailStore');
