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
export class UmbDocumentTypeDetailStore extends UmbStoreBase<DocumentTypeResponseModel> {
	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT.toString(),
			new UmbArrayState<DocumentTypeResponseModel>([], (x) => x.id),
		);
	}

	/**
	 * @param {DocumentTypeResponseModel['id']} id
	 * @return {*}
	 * @memberof UmbDocumentTypeDetailStore
	 */
	byId(id: DocumentTypeResponseModel['id']) {
		return this._data.asObservablePart((x) => x.find((y) => y.id === id));
	}
}

export const UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeDetailStore>(
	'UmbDocumentTypeDetailStore',
);
