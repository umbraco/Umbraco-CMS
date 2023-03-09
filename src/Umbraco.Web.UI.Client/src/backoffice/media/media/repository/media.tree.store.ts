import { EntityTreeItemModel } from '@umbraco-cms/backend-api';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export const UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');

/**
 * @export
 * @class UmbMediaTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Media
 */
export class UmbMediaTreeStore extends UmbTreeStoreBase {
	#data = new ArrayState<EntityTreeItemModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbMediaTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMediaTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
