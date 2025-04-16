import { UMB_DOCUMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS,
		name: 'Document Type Structure Repository',
		api: () => import('./document-type-structure.repository.js'),
	},
];
