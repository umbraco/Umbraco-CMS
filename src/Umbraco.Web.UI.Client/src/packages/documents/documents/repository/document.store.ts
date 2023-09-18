import { DocumentResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbDocumentStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbDocumentDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<DocumentResponseModel>([], (x) => x.id));
	}

	/**
	 * Retrieve a document from the store
	 * @param {string} id
	 * @memberof UmbDocumentStore
	 */
	byId(id: DocumentResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}
}

export const UMB_DOCUMENT_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentStore>('UmbDocumentStore');
