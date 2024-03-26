import type { UmbPartialViewItemModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbPartialViewItemStore
 * @extends {UmbItemStoreBase}
 * @description - Data Store for PartialView items
 */

export class UmbPartialViewItemStore extends UmbItemStoreBase<UmbPartialViewItemModel> {
	/**
	 * Creates an instance of UmbPartialViewItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbPartialViewItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbPartialViewItemStore;

export const UMB_PARTIAL_VIEW_ITEM_STORE_CONTEXT = new UmbContextToken<UmbPartialViewItemStore>(
	'UmbPartialViewItemStore',
);
