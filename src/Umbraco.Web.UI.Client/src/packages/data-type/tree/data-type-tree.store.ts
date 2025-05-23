import { UMB_DATA_TYPE_TREE_STORE_CONTEXT } from './data-type-tree.store.context-token.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDataTypeTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for Data Types
 */
export class UmbDataTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbDataTypeTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_TREE_STORE_CONTEXT.toString());
	}
}
export { UmbDataTypeTreeStore as api };
