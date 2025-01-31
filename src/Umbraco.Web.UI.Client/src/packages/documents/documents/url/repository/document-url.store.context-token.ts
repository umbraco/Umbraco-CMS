import type UmbDocumentUrlStore from './document-url.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_URL_STORE_CONTEXT = new UmbContextToken<UmbDocumentUrlStore>('UmbDocumentUrlStore');
