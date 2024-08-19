import { UMB_TEMPLATE_TREE_STORE_CONTEXT } from './template-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbTemplateTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for Template Items
 */
export class UmbTemplateTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbTemplateTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemplateTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPLATE_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbTemplateTreeStore;
