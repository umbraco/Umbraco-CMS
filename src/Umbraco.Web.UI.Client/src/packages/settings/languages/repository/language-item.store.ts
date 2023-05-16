import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbItemStore } from '@umbraco-cms/backoffice/store';

export const UMB_LANGUAGE_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLanguageItemStore>('UmbLanguageItemStore');

/**
 * @export
 * @class UmbLanguageItemStore
 * @extends {UmbStoreBase}
 * @description -  Store for Languages items
 */
export class UmbLanguageItemStore
	extends UmbStoreBase<LanguageResponseModel>
	implements UmbItemStore<LanguageResponseModel>
{
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_LANGUAGE_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<LanguageResponseModel>([], (x) => x.isoCode)
		);
	}

	items(isoCodes: Array<string>) {
		return this._data.getObservablePart((items) => items.filter((item) => isoCodes.includes(item.isoCode ?? '')));
	}
}
