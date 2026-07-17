import { UMB_ELEMENT_CONFIGURATION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_CONFIGURATION_REPOSITORY_ALIAS,
		name: 'Element Configuration Repository',
		api: () => import('./configuration.repository.js'),
	},
];
