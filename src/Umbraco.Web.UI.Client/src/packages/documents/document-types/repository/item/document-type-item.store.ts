import type { UmbDocumentTypeItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDocumentTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Type items
 */

export class UmbDocumentTypeItemStore extends UmbItemStoreBase<UmbDocumentTypeItemModel> {
	/**
	 * Creates an instance of UmbDocumentTypeItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTypeItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeItemStore>(
	'UmbDocumentTypeItemStore',
);
