import type { UmbMemberGroupItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMemberGroupItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Group items
 */

export class UmbMemberGroupItemStore extends UmbItemStoreBase<UmbMemberGroupItemModel> {
	/**
	 * Creates an instance of UmbMemberGroupItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberGroupItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_GROUP_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbMemberGroupItemStore;

export const UMB_MEMBER_GROUP_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMemberGroupItemStore>(
	'UmbMemberGroupItemStore',
);
