import type { UmbElementFolderItemModel } from './types.js';
import { UMB_ELEMENT_FOLDER_ITEM_STORE_CONTEXT } from './element-folder-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbElementFolderItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Element Folder items
 */

export class UmbElementFolderItemStore extends UmbItemStoreBase<UmbElementFolderItemModel> {
	/**
	 * Creates an instance of UmbElementFolderItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementFolderItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_FOLDER_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbElementFolderItemStore as api };
