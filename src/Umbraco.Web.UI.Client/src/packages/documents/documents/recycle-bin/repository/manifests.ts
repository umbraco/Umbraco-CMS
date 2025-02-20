import { UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from './constants.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		name: 'Document Recycle Bin Repository',
		api: () => import('./document-recycle-bin.repository.js'),
	},
];
