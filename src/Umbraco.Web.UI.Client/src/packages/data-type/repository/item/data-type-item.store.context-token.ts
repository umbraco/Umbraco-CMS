import type { UmbDataTypeItemStore } from './data-type-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DATA_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDataTypeItemStore>('UmbDataTypeItemStore');
