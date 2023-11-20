import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMediaTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Media Items
 */
export class UmbMediaTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbMediaTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_MEDIA_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');
