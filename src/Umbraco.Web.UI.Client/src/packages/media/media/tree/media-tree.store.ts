import { UMB_MEDIA_TREE_STORE_CONTEXT } from './media-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMediaTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Media Items
 */
export class UmbMediaTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMediaTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbMediaTreeStore;
