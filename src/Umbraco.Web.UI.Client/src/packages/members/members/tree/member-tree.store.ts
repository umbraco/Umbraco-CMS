import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbMemberTreeStore
 * @extends {UmbStoreBase}
 * @description - Tree Data Store for Member Items
 */
export class UmbMemberTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbMemberTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_MEMBER_TREE_STORE_CONTEXT = new UmbContextToken<UmbMemberTreeStore>('UmbMemberTreeStore');
