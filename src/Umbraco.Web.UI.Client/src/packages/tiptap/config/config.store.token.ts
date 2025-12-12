import type { UmbTiptapUmbracoPathConfigStore } from './config.store.js';
import { UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_ALIAS } from './constants.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_CONTEXT = new UmbContextToken<UmbTiptapUmbracoPathConfigStore>(
	UMB_TIPTAP_UMBRACO_PATH_CONFIG_STORE_ALIAS,
);
