import type { UmbStylesheetItemStore } from './stylesheet-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_STYLESHEET_ITEM_STORE_CONTEXT = new UmbContextToken<UmbStylesheetItemStore>('UmbStylesheetItemStore');
