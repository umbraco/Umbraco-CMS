import { UMB_USER_CLIENT_CREDENTIAL_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_USER_CLIENT_CREDENTIAL_REPOSITORY_ALIAS,
		name: 'User Client Credentials Repository',
		api: () => import('./user-client-credential.repository.js'),
	},
];
