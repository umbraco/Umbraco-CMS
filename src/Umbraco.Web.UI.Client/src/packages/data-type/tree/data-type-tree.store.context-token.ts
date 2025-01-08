import type { UmbDataTypeTreeStore } from './data-type-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DATA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDataTypeTreeStore>('UmbDataTypeTreeStore');
