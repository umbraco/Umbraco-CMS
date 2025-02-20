import type UmbCurrentUserHistoryStore from './current-user-history.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CURRENT_USER_HISTORY_STORE_CONTEXT = new UmbContextToken<UmbCurrentUserHistoryStore>(
	'UmbCurrentUserHistoryStore',
);
