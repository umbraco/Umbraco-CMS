import type { UmbDocumentDetailModel } from '../../types.js';
import { UMB_DOCUMENT_ITEM_STORE_CONTEXT } from './document-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbDocumentItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document items
 */

export class UmbDocumentItemStore extends UmbItemStoreBase<UmbDocumentDetailModel> {
	/**
	 * Creates an instance of UmbDocumentItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentItemStore as api };
