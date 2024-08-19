import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 
 * @class UmbDataTypeTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for Data Types
 */
export class UmbDataTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDataTypeTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_DATA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDataTypeTreeStore>('UmbDataTypeTreeStore');

export { UmbDataTypeTreeStore as api };
