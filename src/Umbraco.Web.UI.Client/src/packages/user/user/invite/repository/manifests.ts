import { UMB_INVITE_USER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_INVITE_USER_REPOSITORY_ALIAS,
		name: 'Invite User Repository',
		api: () => import('./invite-user.repository.js'),
	},
];
