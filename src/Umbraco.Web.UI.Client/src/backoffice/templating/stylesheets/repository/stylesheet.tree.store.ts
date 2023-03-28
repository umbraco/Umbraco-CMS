import { UmbFileSystemTreeStore } from '@umbraco-cms/backoffice/store';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbStylesheetTreeStore
 * @extends {UmbEntityTreeStore}
 * @description - Tree Data Store for Stylesheets
 */
export class UmbStylesheetTreeStore extends UmbFileSystemTreeStore {
	/**
	 * Creates an instance of UmbStylesheetTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbStylesheetTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbStylesheetTreeStore>(
	'UmbStylesheetTreeStore'
);
