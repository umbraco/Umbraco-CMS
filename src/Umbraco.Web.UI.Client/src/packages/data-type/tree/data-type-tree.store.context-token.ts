import type { UmbDataTypeTreeStore } from './data-type-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbDataTypeTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_DATA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDataTypeTreeStore>('UmbDataTypeTreeStore');
