import type { UmbStaticFileTreeStore } from './static-file-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_STATIC_FILE_TREE_STORE_CONTEXT = new UmbContextToken<UmbStaticFileTreeStore>('UmbStaticFileTreeStore');
