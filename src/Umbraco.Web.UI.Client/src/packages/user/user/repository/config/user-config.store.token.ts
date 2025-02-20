import type { UmbUserConfigStore } from './user-config.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_USER_CONFIG_STORE_CONTEXT = new UmbContextToken<UmbUserConfigStore>('UmbUserConfigStore');
