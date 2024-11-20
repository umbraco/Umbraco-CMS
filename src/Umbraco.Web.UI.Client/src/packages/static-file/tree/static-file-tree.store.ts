import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
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

export const UMB_STATIC_FILE_TREE_STORE_CONTEXT = new UmbContextToken<UmbStaticFileTreeStore>('UmbStaticFileTreeStore');
