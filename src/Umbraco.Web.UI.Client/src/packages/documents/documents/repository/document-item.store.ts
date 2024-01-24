import { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore, UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbDocumentItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document items
 */

export class UmbDocumentItemStore extends UmbEntityItemStore<DocumentItemResponseModel> {
	/**
	 * Creates an instance of UmbDocumentItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_DOCUMENT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentItemStore>('UmbDocumentItemStore');
