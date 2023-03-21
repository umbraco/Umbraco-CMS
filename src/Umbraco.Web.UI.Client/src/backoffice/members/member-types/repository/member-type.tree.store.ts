import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

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
