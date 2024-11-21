import type { UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_URL_STORE_CONTEXT } from './document-url.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbDocumentUrlStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document URLs
 */

export class UmbDocumentUrlStore extends UmbItemStoreBase<UmbDocumentDetailModel> {
	/**
	 * Creates an instance of UmbDocumentUrlStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentUrlStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_URL_STORE_CONTEXT.toString());
	}
}

export default UmbDocumentUrlStore;
