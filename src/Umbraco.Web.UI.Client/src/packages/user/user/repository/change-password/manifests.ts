import { UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const changePasswordRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
	name: 'Change User Password Repository',
	api: () => import('./change-user-password.repository.js'),
};

export const manifests: Array<ManifestTypes> = [changePasswordRepository];
