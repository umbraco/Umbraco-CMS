import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbFileSystemTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbPartialViewTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for PartialView
 */
export class UmbPartialViewTreeStore extends UmbFileSystemTreeStore {
	/**
	 * Creates an instance of UmbPartialViewTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbPartialViewTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT = new UmbContextToken<UmbPartialViewTreeStore>(
	'UmbPartialViewTreeStore',
);
