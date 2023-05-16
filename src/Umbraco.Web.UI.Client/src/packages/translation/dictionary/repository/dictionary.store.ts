import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { DictionaryItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @export
 * @class UmbDictionaryStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Dictionary
 */
export class UmbDictionaryStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_DICTIONARY_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<DictionaryItemResponseModel>([], (x) => x.id)
		);
	}

	append(dictionary: DictionaryItemResponseModel) {
		this._data.append([dictionary]);
	}

	/**
	 * Retrieve a dictionary item from the store
	 * @param {string} id
	 * @memberof UmbDictionaryStore
	 */
	byId(id: DictionaryItemResponseModel['id']) {
		return this._data.getObservablePart((x) => x.find((y) => y.id === id));
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_DICTIONARY_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryStore>('UmbDictionaryStore');
