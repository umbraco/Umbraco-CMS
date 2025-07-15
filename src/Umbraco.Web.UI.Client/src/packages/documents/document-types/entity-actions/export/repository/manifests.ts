import { UMB_EXPORT_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXPORT_DOCUMENT_TYPE_REPOSITORY_ALIAS,
		name: 'Export Document Type Repository',
		api: () => import('./document-type-export.repository.js'),
	},
];
