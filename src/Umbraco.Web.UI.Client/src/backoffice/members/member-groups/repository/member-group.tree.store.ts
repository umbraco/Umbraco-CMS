import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbMemberGroupTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Member Groups
 */
export class UmbMemberGroupTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbMemberGroupTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMemberGroupTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberGroupTreeStore>(
	'UmbMemberGroupTreeStore'
);
