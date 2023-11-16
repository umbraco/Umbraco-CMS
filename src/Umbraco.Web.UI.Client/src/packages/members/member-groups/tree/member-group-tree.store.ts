import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMemberGroupTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for MemberGroup Items
 */
export class UmbMemberGroupTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbMemberGroupTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberGroupTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_MEMBER_GROUP_TREE_STORE_CONTEXT = new UmbContextToken<UmbMemberGroupTreeStore>(
	'UmbMemberGroupTreeStore',
);
