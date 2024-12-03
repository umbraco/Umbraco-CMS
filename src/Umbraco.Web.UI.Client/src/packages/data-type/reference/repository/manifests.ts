import { UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS,
		name: 'Data Type Reference Repository',
		api: () => import('./data-type-reference.repository.js'),
	},
];
