import type { UmbTemplateItemModel } from './types.js';
import { UMB_TEMPLATE_ITEM_STORE_CONTEXT } from './template-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbTemplateItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Template items
 */

export class UmbTemplateItemStore extends UmbItemStoreBase<UmbTemplateItemModel> {
	/**
	 * Creates an instance of UmbTemplateItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemplateItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPLATE_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbTemplateItemStore;
