import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMediaRecycleBinTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Media Recycle Bin Tree Items
 */
export class UmbMediaRecycleBinTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMediaRecycleBinTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaRecycleBinTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_RECYCLE_BIN_TREE_STORE_CONTEXT.toString());
	}
}

export { UmbMediaRecycleBinTreeStore as api };

export const UMB_MEDIA_RECYCLE_BIN_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaRecycleBinTreeStore>(
	'UmbMediaRecycleBinTreeStore',
);
