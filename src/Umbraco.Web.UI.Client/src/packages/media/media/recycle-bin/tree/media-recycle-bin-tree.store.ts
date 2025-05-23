import { UMB_MEDIA_RECYCLE_BIN_TREE_STORE_CONTEXT } from './media-recycle-bin-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbMediaRecycleBinTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for Media Recycle Bin Tree Items
 */
export class UmbMediaRecycleBinTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMediaRecycleBinTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaRecycleBinTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_RECYCLE_BIN_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbMediaRecycleBinTreeStore as api };
