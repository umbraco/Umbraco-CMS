import type { UmbDocumentDetailStore } from './document-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentDetailStore>('UmbDocumentDetailStore');
