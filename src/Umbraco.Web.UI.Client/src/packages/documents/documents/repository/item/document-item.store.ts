import type { UmbDocumentDetailModel } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';
import { UMB_DOCUMENT_ITEM_STORE_CONTEXT } from './document-item.store.context-token.js';

/**
 * @export
 * @class UmbDocumentItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document items
 */

export class UmbDocumentItemStore extends UmbItemStoreBase<UmbDocumentDetailModel> {
	/**
	 * Creates an instance of UmbDocumentItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentItemStore as api };
