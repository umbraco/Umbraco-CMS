import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * @export
 * @class UmbMemberTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Member Types
 */
export class UmbMemberTypeTreeStore extends UmbEntityTreeStore {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeTreeStore>(
	'UmbMemberTypeTreeStore'
);
