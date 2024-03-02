import type { UmbMemberDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbMemberDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Details
 */
export class UmbMemberDetailStore extends UmbDetailStoreBase<UmbMemberDetailModel> {
	/**
	 * Creates an instance of UmbMemberDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMemberDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbMemberDetailStore;

export const UMB_MEMBER_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberDetailStore>('UmbMemberDetailStore');
