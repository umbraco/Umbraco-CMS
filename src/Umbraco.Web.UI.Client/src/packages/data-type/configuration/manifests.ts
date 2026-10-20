import { UMB_DATA_TYPE_CONFIGURATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_CONFIGURATION_REPOSITORY_ALIAS,
		name: 'Data Type Configuration Repository',
		api: () => import('./configuration.repository.js'),
	},
];
