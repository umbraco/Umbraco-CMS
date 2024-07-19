import { UMB_NEW_USER_PASSWORD_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const newPasswordRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_NEW_USER_PASSWORD_REPOSITORY_ALIAS,
	name: 'New User Password Repository',
	api: () => import('./new-user-password.repository.js'),
};

export const manifests: Array<ManifestTypes> = [newPasswordRepository];
