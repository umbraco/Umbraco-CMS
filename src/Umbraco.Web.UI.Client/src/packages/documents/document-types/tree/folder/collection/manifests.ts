import {
	UMB_DOCUMENT_TYPE_FOLDER_COLLECTION_ALIAS,
	UMB_DOCUMENT_TYPE_FOLDER_COLLECTION_REPOSITORY_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_DOCUMENT_TYPE_FOLDER_COLLECTION_ALIAS,
		name: 'Document Type Folder Collection',
		meta: {
			repositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_FOLDER_COLLECTION_REPOSITORY_ALIAS,
		name: 'Document Type Folder Collection Repository',
		api: () => import('./folder-collection.repository.js'),
	},
];
