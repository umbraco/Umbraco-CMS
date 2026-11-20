import { UMB_ELEMENT_CURRENT_USER_PERMISSION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_CURRENT_USER_PERMISSION_REPOSITORY_ALIAS,
		name: 'Element Current User Permission Repository',
		api: () => import('./element-current-user-permission.repository.js'),
	},
];
