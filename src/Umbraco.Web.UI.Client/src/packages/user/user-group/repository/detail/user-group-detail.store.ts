import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_DETAIL_STORE_CONTEXT } from './user-group-detail.store.token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbUserGroupDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for User Group Details
 */
export class UmbUserGroupDetailStore extends UmbDetailStoreBase<UmbUserGroupDetailModel> {
	/**
	 * Creates an instance of UmbUserGroupDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserGroupDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_GROUP_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbUserGroupDetailStore;
