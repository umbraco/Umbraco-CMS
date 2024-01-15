import { TemplateItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityItemStore } from '@umbraco-cms/backoffice/store';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbTemplateItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template items
 */

export class UmbTemplateItemStore extends UmbEntityItemStore<TemplateItemResponseModel> {
	/**
	 * Creates an instance of UmbTemplateItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTemplateItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_TEMPLATE_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_TEMPLATE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbTemplateItemStore>('UmbTemplateItemStore');
