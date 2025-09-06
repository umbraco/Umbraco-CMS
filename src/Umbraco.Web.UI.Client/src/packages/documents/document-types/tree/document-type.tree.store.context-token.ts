import type { UmbDocumentTypeTreeStore } from './document-type.tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * @deprecated - Use `UmbDocumentTypeTreeRepository` instead. This will be removed in Umbraco 18.
 */
export const UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeTreeStore>(
	'UmbDocumentTypeTreeStore',
);
