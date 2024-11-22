import { UMB_DOCUMENT_TYPE_IMPORT_REPOSITORY_ALIAS } from './constants.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_IMPORT_REPOSITORY_ALIAS,
		name: 'Import Document Type Repository',
		api: () => import('./document-type-import.repository.js'),
	},
];
