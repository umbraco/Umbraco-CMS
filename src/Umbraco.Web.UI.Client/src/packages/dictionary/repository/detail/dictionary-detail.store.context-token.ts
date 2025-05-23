import type { UmbDictionaryDetailStore } from './dictionary-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DICTIONARY_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDictionaryDetailStore>(
	'UmbDictionaryDetailStore',
);
