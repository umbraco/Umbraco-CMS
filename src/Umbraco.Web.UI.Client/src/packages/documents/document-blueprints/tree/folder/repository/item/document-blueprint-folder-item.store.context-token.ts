import type { UmbDocumentBlueprintFolderItemStore } from './document-blueprint-folder-item.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_CONTEXT =
	new UmbContextToken<UmbDocumentBlueprintFolderItemStore>('UmbDocumentBlueprintFolderItemStore');
