import { UMB_DOCUMENT_REDIRECT_MANAGEMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_REDIRECT_MANAGEMENT_REPOSITORY_ALIAS,
		name: 'Document Redirect Management Repository',
		api: () => import('./document-redirect-management.repository.js'),
	},
];
