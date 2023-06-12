import { TemplateItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbTemplateItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template items
 */

export class UmbTemplateItemStore
	extends UmbStoreBase<TemplateItemResponseModel>
	implements UmbItemStore<TemplateItemResponseModel>
{
	/**
	 * Creates an instance of UmbTemplateItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(
			host,
			UMB_TEMPLATE_ITEM_STORE_CONTEXT_TOKEN.toString(),
			new UmbArrayState<TemplateItemResponseModel>([], (x) => x.id)
		);
	}

	items(ids: Array<string>) {
		return this._data.getObservablePart((items) => items.filter((item) => ids.includes(item.id ?? '')));
	}
}

export const UMB_TEMPLATE_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateItemStore>('UmbTemplateItemStore');
