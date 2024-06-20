import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbDocumentDetailStore } from './document-detail.store.js';

export const UMB_DOCUMENT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentDetailStore>('UmbDocumentDetailStore');
