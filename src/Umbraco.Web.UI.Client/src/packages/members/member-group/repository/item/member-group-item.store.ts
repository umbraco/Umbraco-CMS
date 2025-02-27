import type { UmbMemberGroupItemModel } from './types.js';
import { UMB_MEMBER_GROUP_ITEM_STORE_CONTEXT } from './member-group-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbMemberGroupItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Member Group items
 */

export class UmbMemberGroupItemStore extends UmbItemStoreBase<UmbMemberGroupItemModel> {
	/**
	 * Creates an instance of UmbMemberGroupItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberGroupItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_GROUP_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbMemberGroupItemStore;
