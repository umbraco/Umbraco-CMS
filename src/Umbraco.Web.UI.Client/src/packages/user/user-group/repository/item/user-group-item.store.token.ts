import type UmbUserGroupItemStore from './user-group-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_GROUP_ITEM_STORE_CONTEXT = new UmbContextToken<UmbUserGroupItemStore>('UmbUserGroupItemStore');
