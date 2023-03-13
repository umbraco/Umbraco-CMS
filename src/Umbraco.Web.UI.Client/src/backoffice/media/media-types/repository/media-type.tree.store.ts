import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbMediaTypeTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Media Types
 */
export class UmbMediaTypeTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbMediaTypeTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMediaTypeTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeTreeStore>(
	'UmbMediaTypeTreeStore'
);
