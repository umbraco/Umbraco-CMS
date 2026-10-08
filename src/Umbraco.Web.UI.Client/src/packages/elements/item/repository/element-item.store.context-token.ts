import type { UmbElementItemStore } from './element-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ELEMENT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbElementItemStore>('UmbElementItemStore');
