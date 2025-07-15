import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS, UMB_DOCUMENT_DETAIL_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
		name: 'Document Detail Repository',
		api: () => import('./document-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_DETAIL_STORE_ALIAS,
		name: 'Document Detail Store',
		api: () => import('./document-detail.store.js'),
	},
];
