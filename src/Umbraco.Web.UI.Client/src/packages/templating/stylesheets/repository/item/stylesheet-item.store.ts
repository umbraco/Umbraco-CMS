import type { StylesheetItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbStylesheetItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Stylesheet items
 */

export class UmbStylesheetItemStore
	extends UmbStoreBase<StylesheetItemResponseModel>
	implements UmbItemStore<StylesheetItemResponseModel>
{
	/**
	 * Creates an instance of UmbStylesheetItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<StylesheetItemResponseModel>([], (x) => x.path),
		);
	}

	items(ids: Array<string>) {
		return this._data.asObservablePart((items) => items.filter((item) => ids.includes(item.path ?? '')));
	}
}

export const UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbStylesheetItemStore>(
	'UmbStylesheetItemStore',
);
