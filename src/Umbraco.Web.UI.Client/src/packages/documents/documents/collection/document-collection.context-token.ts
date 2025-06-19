import type { UmbDocumentCollectionContext } from './document-collection.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_COLLECTION_CONTEXT = new UmbContextToken<UmbDocumentCollectionContext>(
	'UmbCollectionContext',
);
