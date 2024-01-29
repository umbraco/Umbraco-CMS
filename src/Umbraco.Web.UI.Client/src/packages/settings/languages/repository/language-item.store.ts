import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';

export const UMB_LANGUAGE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbLanguageItemStore>('UmbLanguageItemStore');

/**
 * @export
 * @class UmbLanguageItemStore
 * @extends {UmbStoreBase}
 * @description -  Store for Languages items
 */
export class UmbLanguageItemStore extends UmbStoreBase<LanguageResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_LANGUAGE_ITEM_STORE_CONTEXT.toString(),
			new UmbArrayState<LanguageResponseModel>([], (x) => x.isoCode),
		);
	}

	items(isoCodes: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => isoCodes.includes(item.isoCode ?? '')));
	}
}
