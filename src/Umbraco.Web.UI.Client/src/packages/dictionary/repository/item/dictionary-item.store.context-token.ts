import type { UmbDictionaryItemStore } from './dictionary-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DICTIONARY_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDictionaryItemStore>('UmbDictionaryItemStore');
