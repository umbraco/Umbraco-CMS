import type { UmbMemberDetailModel } from '../../types.js';
import { UMB_MEMBER_DETAIL_STORE_CONTEXT } from './member-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbMemberDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Member Details
 */
export class UmbMemberDetailStore extends UmbDetailStoreBase<UmbMemberDetailModel> {
	/**
	 * Creates an instance of UmbMemberDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbMemberDetailStore;
