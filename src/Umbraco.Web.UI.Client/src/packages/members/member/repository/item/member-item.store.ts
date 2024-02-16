import type { UmbMemberItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMemberItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member items
 */

export class UmbMemberItemStore extends UmbItemStoreBase<UmbMemberItemModel> {
	/**
	 * Creates an instance of UmbMemberItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_ITEM_STORE_CONTEXT.toString());
	}
}

export const UMB_MEMBER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMemberItemStore>('UmbMemberItemStore');
