import { UMB_ELEMENT_ROLLBACK_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_ROLLBACK_REPOSITORY_ALIAS,
		name: 'Element Rollback Repository',
		api: () => import('./rollback.repository.js'),
	},
];
