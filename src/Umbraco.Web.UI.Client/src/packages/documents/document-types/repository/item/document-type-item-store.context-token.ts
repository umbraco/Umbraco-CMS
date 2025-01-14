import type { UmbDocumentTypeItemStore } from './document-type-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeItemStore>(
	'UmbDocumentTypeItemStore',
);
