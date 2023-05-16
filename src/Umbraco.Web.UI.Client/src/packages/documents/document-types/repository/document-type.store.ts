import { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeStore extends UmbStoreBase {
	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<DocumentTypeResponseModel>([], (x) => x.id)
		);
	}

	/**
	 * Append a document-type to the store
	 * @param {DocumentTypeModel} document
	 * @memberof UmbDocumentTypeStore
	 */
	append(document: DocumentTypeResponseModel) {
		this._data.append([document]);
	}

	/**
	 * Append a document-type to the store
	 * @param {DocumentTypeModel} document
	 * @memberof UmbDocumentTypeStore
	 */
	byId(id: DocumentTypeResponseModel['id']) {
		return this._data.getObservablePart((x) => x.find((y) => y.id === id));
	}

	/**
	 * Removes document-types in the store with the given uniques
	 * @param {string[]} uniques
	 * @memberof UmbDocumentTypeStore
	 */
	remove(uniques: Array<DocumentTypeResponseModel['id']>) {
		this._data.remove(uniques);
	}
}

export const UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeStore>('UmbDocumentTypeStore');
