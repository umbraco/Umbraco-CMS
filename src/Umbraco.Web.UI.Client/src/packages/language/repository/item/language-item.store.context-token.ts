import type { UmbLanguageItemStore } from './language-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_LANGUAGE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbLanguageItemStore>('UmbLanguageItemStore');
