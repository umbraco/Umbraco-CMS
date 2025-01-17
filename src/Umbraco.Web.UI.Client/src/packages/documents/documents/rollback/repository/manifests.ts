import { UMB_ROLLBACK_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ROLLBACK_REPOSITORY_ALIAS,
		name: 'Rollback Repository',
		api: () => import('./rollback.repository.js'),
	},
];
