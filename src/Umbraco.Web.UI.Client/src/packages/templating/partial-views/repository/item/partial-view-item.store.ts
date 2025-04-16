import type { UmbPartialViewItemModel } from '../../types.js';
import { UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT } from './partial-view-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbPartialViewItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for PartialView items
 */

export class UmbPartialViewItemStore extends UmbItemStoreBase<UmbPartialViewItemModel> {
	/**
	 * Creates an instance of UmbPartialViewItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbPartialViewItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbPartialViewItemStore;
