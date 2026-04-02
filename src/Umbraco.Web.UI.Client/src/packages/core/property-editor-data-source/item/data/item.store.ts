import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbPropertyEditorDataSourceItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbPropertyEditorDataSourceItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Property Editor Data Source items
 */

export class UmbPropertyEditorDataSourceItemStore extends UmbItemStoreBase<UmbPropertyEditorDataSourceItemModel> {
	/**
	 * Creates an instance of UmbPropertyEditorDataSourceItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbPropertyEditorDataSourceItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbPropertyEditorDataSourceItemStore as api };
