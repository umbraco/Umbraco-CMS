import type { UmbDocumentDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

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

export const UMB_DOCUMENT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentItemStore>('UmbDocumentItemStore');
