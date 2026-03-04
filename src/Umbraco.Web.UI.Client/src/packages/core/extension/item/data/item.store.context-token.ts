import type { UmbExtensionItemStore } from './item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_EXTENSION_ITEM_STORE_CONTEXT = new UmbContextToken<UmbExtensionItemStore>('UmbExtensionItemStore');
