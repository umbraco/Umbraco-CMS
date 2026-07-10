import { UMB_DUPLICATE_ELEMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DUPLICATE_ELEMENT_REPOSITORY_ALIAS,
		name: 'Duplicate Element Repository',
		api: () => import('./element-duplicate.repository.js'),
	},
];
