import { UMB_CURRENT_USER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_CURRENT_USER_REPOSITORY_ALIAS,
		name: 'Current User Repository',
		api: () => import('./current-user.repository.js'),
	},
	{
		type: 'store',
		alias: 'Umb.Store.CurrentUser',
		name: 'Current User Store',
		api: () => import('./current-user.store.js'),
	},
];
