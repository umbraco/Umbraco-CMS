import type { UmbMemberItemStore } from './member-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMemberItemStore>('UmbMemberItemStore');
