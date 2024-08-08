import { UMB_SCRIPT_TREE_STORE_CONTEXT } from './script-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbScriptTreeStore
 * @augments {UmbUniqueTreeStore}
 * @description - Tree Data Store for Scripts
 */
export class UmbScriptTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbScriptTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbScriptTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbScriptTreeStore;
