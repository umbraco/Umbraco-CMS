import type { UmbCurrentUserConfigStore } from './current-user-config.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CURRENT_USER_CONFIG_STORE_CONTEXT = new UmbContextToken<UmbCurrentUserConfigStore>(
	'UmbCurrentUserConfigStore',
);
