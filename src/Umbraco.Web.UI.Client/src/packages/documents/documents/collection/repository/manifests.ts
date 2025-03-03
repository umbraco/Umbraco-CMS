import { UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_COLLECTION_REPOSITORY_ALIAS,
		name: 'Document Collection Repository',
		api: () => import('./document-collection.repository.js'),
	},
];
