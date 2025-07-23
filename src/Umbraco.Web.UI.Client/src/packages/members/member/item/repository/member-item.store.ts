import type { UmbMemberItemModel } from './types.js';
import { UMB_MEMBER_ITEM_STORE_CONTEXT } from './member-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbMemberItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Member items
 */

export class UmbMemberItemStore extends UmbItemStoreBase<UmbMemberItemModel> {
	/**
	 * Creates an instance of UmbMemberItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbMemberItemStore;
