import { DocumentResponseModel } from '@umbraco-cms/backend-api';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbDocumentDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbDocumentStore extends UmbStoreBase {
	#data = new ArrayState<DocumentResponseModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbDocumentDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentDetailStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DOCUMENT_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a document to the store
	 * @param {DocumentDetails} document
	 * @memberof UmbDocumentDetailStore
	 */
	append(document: DocumentResponseModel) {
		this.#data.append([document]);
	}

	/**
	 * Append a document to the store
	 * @param {DocumentModel} document
	 * @memberof UmbDocumentStore
	 */
	byKey(key: DocumentResponseModel['key']) {
		return this.#data.getObservablePart((x) => x.find((y) => y.key === key));
	}

	/**
	 * Removes documents in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbDocumentDetailStore
	 */
	remove(uniques: Array<DocumentResponseModel['key']>) {
		this.#data.remove(uniques);
	}
}

export const UMB_DOCUMENT_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentStore>('UmbDocumentStore');
