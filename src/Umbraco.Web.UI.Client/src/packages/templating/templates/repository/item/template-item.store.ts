import type { UmbTemplateItemModel } from './types.js';
import { UMB_TEMPLATE_ITEM_STORE_CONTEXT } from './template-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbTemplateItemStore
 * @augments {UmbStoreBase}
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
