import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMediaTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Media Types
 */
export class UmbMediaTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMediaTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeTreeStore>('UmbMediaTypeTreeStore');
