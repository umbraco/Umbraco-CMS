import { UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS,
		name: 'Duplicate Data Type Repository',
		api: () => import('./data-type-duplicate.repository.js'),
	},
];
