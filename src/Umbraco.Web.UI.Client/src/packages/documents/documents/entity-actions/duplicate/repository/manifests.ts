import { UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
		name: 'Duplicate Document Repository',
		api: () => import('./document-duplicate.repository.js'),
	},
];
