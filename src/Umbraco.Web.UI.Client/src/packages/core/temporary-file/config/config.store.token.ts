import type { UmbTemporaryFileConfigStore } from './config.store.js';
import { UMB_TEMPORARY_FILE_CONFIG_STORE_ALIAS } from './constants.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT = new UmbContextToken<UmbTemporaryFileConfigStore>(
	UMB_TEMPORARY_FILE_CONFIG_STORE_ALIAS,
);
