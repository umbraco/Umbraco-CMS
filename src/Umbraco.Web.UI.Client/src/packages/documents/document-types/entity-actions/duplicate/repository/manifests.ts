import { UMB_DUPLICATE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DUPLICATE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
		name: 'Duplicate Document Type Repository',
		api: () => import('./document-type-duplicate.repository.js'),
	},
];
