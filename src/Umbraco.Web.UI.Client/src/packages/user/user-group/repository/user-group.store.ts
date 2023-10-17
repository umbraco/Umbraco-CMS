import type { UserGroupDetails } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';

export const UMB_USER_GROUP_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbUserGroupStore>('UmbUserGroupStore');

/**
 * @export
 * @class UmbUserGroupStore
 * @extends {UmbStoreBase}
 * @description - Data Store for User Groups
 */
export class UmbUserGroupStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_GROUP_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<UserGroupDetails>([], (x) => x.id));
	}
}
