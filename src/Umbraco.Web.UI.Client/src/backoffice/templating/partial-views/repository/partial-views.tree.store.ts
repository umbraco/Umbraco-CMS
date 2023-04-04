import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbFileSystemTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN_ALIAS } from '../config';

export const UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbPartialViewsTreeStore>(
	UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN_ALIAS
);

/**
 * Tree Store for partial views
 *
 * @export
 * @class UmbPartialViewsTreeStore
 * @extends {UmbEntityTreeStore}
 */
export class UmbPartialViewsTreeStore extends UmbFileSystemTreeStore {
	/**
	 * Creates an instance of UmbPartialViewsTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewsTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
