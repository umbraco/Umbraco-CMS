import type { UmbDocumentTreeStore } from './document-tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentTreeStore');
