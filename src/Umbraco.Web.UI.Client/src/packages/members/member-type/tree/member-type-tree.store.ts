import { UMB_MEMBER_TYPE_TREE_STORE_CONTEXT } from './member-type-tree.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMemberTypeTreeStore
 * @augments {UmbStoreBase}
 * @description - Tree Data Store for MemberType Items
 */
export class UmbMemberTypeTreeStore extends UmbUniqueTreeStore {
	/**
	 * Creates an instance of UmbMemberTypeTreeStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTypeTreeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT.toString());
	}
}

export default UmbMemberTypeTreeStore;
