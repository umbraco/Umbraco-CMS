import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { DictionaryDetails } from '../';

/**
 * @export
 * @class UmbDictionaryStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Dictionary
 */
export class UmbDictionaryStore extends UmbStoreBase {
	#data = new ArrayState<DictionaryDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_DICTIONARY_STORE_CONTEXT_TOKEN.toString());
	}

	append(dictionary: DictionaryDetails) {
		this.#data.append([dictionary]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_DICTIONARY_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryStore>('UmbDictionaryStore');
