import type { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDocumentTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Type items
 */

export class UmbDocumentTypeItemStore extends UmbEntityItemStore<DocumentTypeItemResponseModel> {
	/**
	 * Creates an instance of UmbDocumentTypeItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeItemStore>(
	'UmbDocumentTypeItemStore',
);
