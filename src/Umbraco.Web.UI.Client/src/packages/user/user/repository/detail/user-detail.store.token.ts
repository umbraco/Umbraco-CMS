import type UmbUserDetailStore from './user-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbUserDetailStore>('UmbUserDetailStore');
