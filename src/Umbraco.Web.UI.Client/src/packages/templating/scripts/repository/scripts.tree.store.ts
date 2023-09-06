import { UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN_ALIAS } from '../config.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbFileSystemTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export const UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbScriptsTreeStore>(
	UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN_ALIAS,
);

/**
 * Tree Store for scripts
 *
 * @export
 * @class 
 * @extends {UmbEntityTreeStore}
 */
export class UmbScriptsTreeStore extends UmbFileSystemTreeStore {
	/**
	 * Creates an instance of UmbScriptsTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbScriptsTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
