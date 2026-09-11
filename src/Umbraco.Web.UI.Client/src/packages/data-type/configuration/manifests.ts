import { UMB_DATA_TYPES_CONFIGURATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPES_CONFIGURATION_REPOSITORY_ALIAS,
		name: 'Data Types Configuration Repository',
		api: () => import('./configuration.repository.js'),
	},
];
