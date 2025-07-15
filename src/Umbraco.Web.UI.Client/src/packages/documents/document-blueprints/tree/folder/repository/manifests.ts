import {
	UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
		name: 'Document Blueprint Folder Repository',
		api: () => import('./document-blueprint-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_BLUEPRINT_FOLDER_STORE_ALIAS,
		name: 'Document Blueprint Folder Store',
		api: () => import('./document-blueprint-folder.store.js'),
	},
];
