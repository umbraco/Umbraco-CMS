import { UMB_STATIC_FILE_TREE_STORE_CONTEXT } from './static-file-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbStaticFileTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for Static File Items
 */
export class UmbStaticFileTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbStaticFileTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStaticFileTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STATIC_FILE_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbStaticFileTreeStore;
