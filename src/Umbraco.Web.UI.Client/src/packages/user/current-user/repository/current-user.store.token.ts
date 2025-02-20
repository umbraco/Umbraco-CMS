import type UmbCurrentUserStore from './current-user.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CURRENT_USER_STORE_CONTEXT = new UmbContextToken<UmbCurrentUserStore>('UmbCurrentUserStore');
