import { UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT } from './partial-view-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbPartialViewTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for PartialView
 */
export class UmbPartialViewTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbPartialViewTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbPartialViewTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbPartialViewTreeStore;
