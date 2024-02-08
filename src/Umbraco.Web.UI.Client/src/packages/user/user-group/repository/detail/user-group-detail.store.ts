import type { UmbUserGroupDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbUserGroupDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for User Group Details
 */
export class UmbUserGroupDetailStore extends UmbDetailStoreBase<UmbUserGroupDetailModel> {
	/**
	 * Creates an instance of UmbUserGroupDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserGroupDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_GROUP_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_USER_GROUP_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbUserGroupDetailStore>(
	'UmbUserGroupDetailStore',
);
