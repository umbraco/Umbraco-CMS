import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_FOLDER_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Document Type Folder Repository',
		api: () => import('./document-type-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_TYPE_FOLDER_STORE_ALIAS,
		name: 'Document Type Folder Store',
		api: () => import('./document-type-folder.store.js'),
	},
];
