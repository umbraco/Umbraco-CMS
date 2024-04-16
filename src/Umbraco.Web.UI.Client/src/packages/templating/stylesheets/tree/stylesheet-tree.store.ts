import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbStylesheetTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Stylesheets
 */
export class UmbStylesheetTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbStylesheetTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbStylesheetTreeStore;

export const UMB_STYLESHEET_TREE_STORE_CONTEXT = new UmbContextToken<UmbStylesheetTreeStore>('UmbStylesheetTreeStore');
