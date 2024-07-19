import type { UmbDocumentRecycleBinTreeStore } from './document-recycle-bin-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentRecycleBinTreeStore>(
	'UmbDocumentRecycleBinTreeStore',
);
