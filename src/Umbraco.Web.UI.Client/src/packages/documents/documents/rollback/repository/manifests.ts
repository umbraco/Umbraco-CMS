import { UMB_DOCUMENT_ROLLBACK_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_ROLLBACK_REPOSITORY_ALIAS,
		name: 'Document Rollback Repository',
		api: () => import('./rollback.repository.js'),
	},
];
