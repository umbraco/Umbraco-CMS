import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const UMB_LANGUAGE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageStore>('UmbLanguageStore');

/**
 * @export
 * @class UmbLanguageStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Languages
 */
export class UmbLanguageStore extends UmbStoreBase {
	#data = new ArrayState<LanguageResponseModel>([], (x) => x.isoCode);
	data = this.#data.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_LANGUAGE_STORE_CONTEXT_TOKEN.toString());
	}

	append(language: LanguageResponseModel) {
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
