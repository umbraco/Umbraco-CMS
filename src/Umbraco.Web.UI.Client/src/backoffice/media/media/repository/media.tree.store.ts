import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbTreeStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export const UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');

/**
 * @export
 * @class UmbMediaTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Media
 */
export class UmbMediaTreeStore extends UmbTreeStoreBase {
	#data = new ArrayState<EntityTreeItemResponseModel>([], (x) => x.key);

	/**
	 * Creates an instance of UmbMediaTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMediaTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
