import { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbDocumentTypeItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Type items
 */

export class UmbDocumentTypeItemStore
	extends UmbStoreBase<DocumentTypeItemResponseModel>
	implements UmbItemStore<DocumentTypeItemResponseModel>
{
	/**
	 * Creates an instance of UmbDocumentTypeItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<DocumentTypeItemResponseModel>([], (x) => x.id),
		);
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentTypeItemStore>(
	'UmbDocumentTypeItemStore',
);
