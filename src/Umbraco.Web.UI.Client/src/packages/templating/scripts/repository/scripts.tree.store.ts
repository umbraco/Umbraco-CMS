import { UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN_ALIAS } from '../config.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbFileSystemTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export const UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbScriptsTreeStore>(
	UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN_ALIAS,
);

/**
 * Tree Store for partial views
 *
 * @export
 * @class UmbPartialViewsTreeStore
 * @extends {UmbEntityTreeStore}
 */
export class UmbScriptsTreeStore extends UmbFileSystemTreeStore {
	/**
	 * Creates an instance of UmbPartialViewsTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewsTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
