import type { UmbMemberDetailStore } from './member-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbMemberDetailStore>('UmbMemberDetailStore');
