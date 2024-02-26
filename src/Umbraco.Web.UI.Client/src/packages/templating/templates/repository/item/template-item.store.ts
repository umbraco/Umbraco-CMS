import type { UmbTemplateItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbTemplateItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template items
 */

export class UmbTemplateItemStore extends UmbItemStoreBase<UmbTemplateItemModel> {
	/**
	 * Creates an instance of UmbTemplateItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPLATE_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbTemplateItemStore;

export const UMB_TEMPLATE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbTemplateItemStore>('UmbTemplateItemStore');
