import type { UmbDocumentItemStore } from './document-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentItemStore>('UmbDocumentItemStore');
