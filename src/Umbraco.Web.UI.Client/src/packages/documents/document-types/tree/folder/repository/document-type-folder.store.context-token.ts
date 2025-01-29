import type { UmbDocumentTypeFolderStore } from './document-type-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_TYPE_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeFolderStore>(
	'UmbDocumentTypeFolderStore',
);
