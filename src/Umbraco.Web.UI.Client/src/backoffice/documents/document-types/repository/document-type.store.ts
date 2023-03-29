import { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbDocumentTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeStore extends UmbStoreBase {
	#data = new ArrayState<DocumentTypeResponseModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * Append a document-type to the store
	 * @param {DocumentTypeModel} document
	 * @memberof UmbDocumentTypeStore
	 */
	append(document: DocumentTypeResponseModel) {
		this.#data.append([document]);
	}

	/**
	 * Append a document-type to the store
	 * @param {DocumentTypeModel} document
	 * @memberof UmbDocumentTypeStore
	 */
	byKey(key: DocumentTypeResponseModel['key']) {
		return this.#data.getObservablePart((x) => x.find((y) => y.key === key));
	}

	/**
	 * Removes document-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbDocumentTypeStore
	 */
	remove(uniques: Array<DocumentTypeResponseModel['key']>) {
		this.#data.remove(uniques);
	}
}

export const UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeStore>('UmbDocumentTypeStore');
