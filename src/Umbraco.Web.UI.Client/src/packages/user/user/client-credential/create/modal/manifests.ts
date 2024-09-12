import { UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL_ALIAS } from './constants.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'modal',
		alias: UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL_ALIAS,
		name: 'Create User Client Credential Modal',
		js: () => import('./create-user-client-credential-modal.element.js'),
	},
];
