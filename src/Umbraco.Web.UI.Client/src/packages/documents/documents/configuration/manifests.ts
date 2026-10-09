import { UMB_DOCUMENT_CONFIGURATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_CONFIGURATION_REPOSITORY_ALIAS,
		name: 'Document Configuration Repository',
		api: () => import('./configuration.repository.js'),
	},
];
