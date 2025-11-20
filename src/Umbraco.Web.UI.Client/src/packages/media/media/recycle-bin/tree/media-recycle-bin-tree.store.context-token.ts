import type { UmbMediaRecycleBinTreeStore } from './media-recycle-bin-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbMediaRecycleBinTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_MEDIA_RECYCLE_BIN_TREE_STORE_CONTEXT = new UmbContextToken<UmbMediaRecycleBinTreeStore>(
	'UmbMediaRecycleBinTreeStore',
);
