import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMemberTypeTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for MemberType Items
 */
export class UmbMemberTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMemberTypeTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberTypeTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_MEMBER_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMemberTypeTreeStore>('UmbMemberTypeTreeStore');
