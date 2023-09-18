import { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbDocumentItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document items
 */

export class UmbDocumentItemStore
	extends UmbStoreBase<DocumentItemResponseModel>
	implements UmbItemStore<DocumentItemResponseModel>
{
	/**
	 * Creates an instance of UmbDocumentItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DOCUMENT_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<DocumentItemResponseModel>([], (x) => x.id),
		);
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_DOCUMENT_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDocumentItemStore>('UmbDocumentItemStore');
