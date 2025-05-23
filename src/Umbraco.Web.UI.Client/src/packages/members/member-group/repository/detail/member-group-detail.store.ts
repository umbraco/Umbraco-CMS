import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT } from './member-group-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbMemberGroupDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Member Group Details
 */
export class UmbMemberGroupDetailStore extends UmbDetailStoreBase<UmbMemberGroupDetailModel> {
	/**
	 * Creates an instance of UmbMemberGroupDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberGroupDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbMemberGroupDetailStore;
