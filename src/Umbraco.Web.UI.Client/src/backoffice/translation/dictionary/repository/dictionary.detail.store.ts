import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ArrayState } from '@umbraco-cms/observable-api';
import type { DictionaryDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbDictionaryDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Dictionary
 */
export class UmbDictionaryDetailStore
	extends UmbStoreBase
{
	#data = new ArrayState<DictionaryDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UmbDictionaryDetailStore.name);
	}

	append(dictionary: DictionaryDetails) {
		this.#data.append([dictionary]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}	
}

export const UMB_DICTIONARY_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbDictionaryDetailStore>(
	UmbDictionaryDetailStore.name
);