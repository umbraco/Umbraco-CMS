import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

export const UMB_MEMBER_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTreeStore>('UmbMemberTreeStore');

/**
 * @export
 * @class UmbMemberTreeStore
 * @extends {UmbTreeStoreBase}
 * @description - Tree Data Store for Members
 */
export class UmbMemberTreeStore extends UmbTreeStoreBase {
	/**
	 * Creates an instance of UmbTemplateTreeStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbMemberGroupTreeStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}
