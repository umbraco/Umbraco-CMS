import { UMB_USER_CLIENT_CREDENTIAL_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_CLIENT_CREDENTIAL_REPOSITORY_ALIAS,
		name: 'User Client Credentials Repository',
		api: () => import('./user-client-credential.repository.js'),
	},
];
