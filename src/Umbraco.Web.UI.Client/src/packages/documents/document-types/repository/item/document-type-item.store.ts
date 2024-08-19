import type { UmbDocumentTypeItemModel } from './types.js';
import { UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT } from './document-type-item-store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbDocumentTypeItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document Type items
 */

export class UmbDocumentTypeItemStore extends UmbItemStoreBase<UmbDocumentTypeItemModel> {
	/**
	 * Creates an instance of UmbDocumentTypeItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentTypeItemStore as api };
