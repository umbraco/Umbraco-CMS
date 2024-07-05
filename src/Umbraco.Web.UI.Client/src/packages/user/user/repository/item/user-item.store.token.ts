import type UmbUserItemStore from './user-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_ITEM_STORE_CONTEXT = new UmbContextToken<UmbUserItemStore>('UmbUserItemStore');
