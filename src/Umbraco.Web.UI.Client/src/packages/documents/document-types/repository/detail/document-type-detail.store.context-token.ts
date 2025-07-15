import type { UmbDocumentTypeDetailStore } from './document-type-detail.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeDetailStore>(
	'UmbDocumentTypeDetailStore',
);
