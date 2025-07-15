import type UmbUserGroupDetailStore from './user-group-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_GROUP_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbUserGroupDetailStore>(
	'UmbUserGroupDetailStore',
);
