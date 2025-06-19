import { UMB_STYLESHEET_TREE_STORE_CONTEXT } from './stylesheet-tree.store.context-token.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbStylesheetTreeStore
 * @augments {UmbUniqueTreeStore}
 * @description - Tree Data Store for Stylesheets
 */
export class UmbStylesheetTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbStylesheetTreeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStylesheetTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbStylesheetTreeStore;
