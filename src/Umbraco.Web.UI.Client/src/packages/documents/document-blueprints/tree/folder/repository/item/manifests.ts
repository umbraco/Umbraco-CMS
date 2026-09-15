import {
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_ALIAS,
} from './constants.js';
import { UmbDocumentBlueprintFolderItemStore } from './document-blueprint-folder-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_REPOSITORY_ALIAS,
		name: 'Document Blueprint Folder Item Repository',
		api: () => import('./document-blueprint-folder-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_BLUEPRINT_FOLDER_ITEM_STORE_ALIAS,
		name: 'Document Blueprint Folder Item Store',
		api: UmbDocumentBlueprintFolderItemStore,
	},
];
