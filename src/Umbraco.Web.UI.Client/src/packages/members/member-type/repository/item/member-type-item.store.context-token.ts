import type { UmbMemberTypeItemStore } from './member-type-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMBER_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbMemberTypeItemStore>('UmbMemberTypeItemStore');
