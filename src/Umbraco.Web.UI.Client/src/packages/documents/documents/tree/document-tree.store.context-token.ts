import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDocumentTreeStore } from './document-tree.store.js';

export const UMB_DOCUMENT_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentTreeStore>('UmbDocumentTreeStore');
