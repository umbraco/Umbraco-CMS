import type { UmbMemberTypeDetailStore } from './member-type-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberTypeDetailStore>(
	'UmbMemberTypeDetailStore',
);
