import { DocumentTypeModel } from '@umbraco-cms/backend-api';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbDocumentTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeStore extends UmbStoreBase {
	#data = new ArrayState<DocumentTypeModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UmbDocumentTypeStore.name);
	}

	/**
	 * Append a document-type to the store
	 * @param {DocumentTypeModel} document
	 * @memberof UmbDocumentTypeStore
	 */
	append(document: DocumentTypeModel) {
		this.#data.append([document]);
	}

	/**
	 * Append a document-type to the store
	 * @param {DocumentTypeModel} document
	 * @memberof UmbDocumentTypeStore
	 */
	byKey(key: DocumentTypeModel['key']) {
		return this.#data.getObservablePart((x) => x.find((y) => y.key === key));
	}

	/**
	 * Removes document-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbDocumentTypeStore
	 */
	remove(uniques: Array<DocumentTypeModel['key']>) {
		this.#data.remove(uniques);
	}
}

export const UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeStore>(
	UmbDocumentTypeStore.name
);
