import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { DictionaryItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @export
 * @class UmbDictionaryStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Dictionary
 */
export class UmbDictionaryStore extends UmbStoreBase {
	#data = new ArrayState<DictionaryItemResponseModel>([], (x) => x.id);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DICTIONARY_STORE_CONTEXT_TOKEN.toString());
	}

	append(dictionary: DictionaryItemResponseModel) {
		this.#data.append([dictionary]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_DICTIONARY_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryStore>('UmbDictionaryStore');
