import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbMediaTypeTreeStore
 * @extends {UmbEntityTreeStore}
 * @description - Tree Data Store for Media Types
 */
export class UmbMediaTypeTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbMediaTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeTreeStore>(
	'UmbMediaTypeTreeStore'
);
