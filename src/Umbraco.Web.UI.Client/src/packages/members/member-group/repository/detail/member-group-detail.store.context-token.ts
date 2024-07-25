import type { UmbMemberGroupDetailStore } from './member-group-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberGroupDetailStore>(
	'UmbMemberGroupDetailStore',
);
