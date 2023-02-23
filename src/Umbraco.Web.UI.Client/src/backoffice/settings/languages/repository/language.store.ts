import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ArrayState } from '@umbraco-cms/observable-api';
import { LanguageModel } from '@umbraco-cms/backend-api';

/**
 * @export
 * @class UmbLanguageStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Languages
 */
export class UmbLanguageStore extends UmbStoreBase {
	#data = new ArrayState<LanguageModel>([], (x) => x.isoCode);
	data = this.#data.asObservable();

	constructor(host: UmbControllerHostInterface) {
		super(host, UmbLanguageStore.name);
	}

	append(language: LanguageModel) {
		this.#data.append([language]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}

	// TODO: how do we best handle this? They might have a smaller data set than the details
	items(isoCodes: Array<string>) {
		return this.#data.getObservablePart((items) => items.filter((item) => isoCodes.includes(item.isoCode ?? '')));
	}
}

export const UMB_LANGUAGE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageStore>(UmbLanguageStore.name);
