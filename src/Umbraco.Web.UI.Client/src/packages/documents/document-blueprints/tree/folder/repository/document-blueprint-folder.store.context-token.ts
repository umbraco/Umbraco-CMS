import type { UmbDocumentBlueprintFolderStore } from './document-blueprint-folder.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintFolderStore>(
	'UmbDocumentBlueprintFolderStore',
);
