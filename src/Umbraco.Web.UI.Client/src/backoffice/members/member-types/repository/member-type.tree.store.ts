import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbMemberTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Member Types
 */
export class UmbMemberTypeTreeStore extends UmbTreeStoreBase {
	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeTreeStore>(
	'UmbMemberTypeTreeStore'
);
