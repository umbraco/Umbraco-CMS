import type UmbMediaTreeStore from './media-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbMediaTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_MEDIA_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaTreeStore>('UmbMediaTreeStore');
