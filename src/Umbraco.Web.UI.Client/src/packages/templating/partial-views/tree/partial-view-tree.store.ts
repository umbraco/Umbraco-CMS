import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbPartialViewTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for PartialView
 */
export class UmbPartialViewTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbPartialViewTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbPartialViewTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbPartialViewTreeStore;

export const UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT = new UmbContextToken<UmbPartialViewTreeStore>(
	'UmbPartialViewTreeStore',
);
