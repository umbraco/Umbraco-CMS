import { UMB_IMAGING_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_IMAGING_REPOSITORY_ALIAS,
		name: 'Imaging Repository',
		api: () => import('./imaging.repository.js'),
	},
];
