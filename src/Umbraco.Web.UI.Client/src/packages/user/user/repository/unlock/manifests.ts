import { UMB_UNLOCK_USER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_UNLOCK_USER_REPOSITORY_ALIAS,
		name: 'Unlock User Repository',
		api: () => import('./unlock-user.repository.js'),
	},
];
