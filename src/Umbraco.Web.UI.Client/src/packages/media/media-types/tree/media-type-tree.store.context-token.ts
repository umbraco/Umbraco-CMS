import type UmbMediaTypeTreeStore from './media-type-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbMediaTypeTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_MEDIA_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTypeTreeStore>('UmbMediaTypeTreeStore');
