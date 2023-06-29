import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const UMB_LANGUAGE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageStore>('UmbLanguageStore');

/**
 * @export
 * @class UmbLanguageStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Languages
 */
export class UmbLanguageStore extends UmbStoreBase {
	public readonly data = this._data.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_LANGUAGE_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<LanguageResponseModel>([], (x) => x.isoCode)
		);
	}

	append(language: LanguageResponseModel) {
		this._data.append([language]);
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}

	// TODO: how do we best handle this? They might have a smaller data set than the details
	items(isoCodes: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => isoCodes.includes(item.isoCode ?? '')));
	}
}
