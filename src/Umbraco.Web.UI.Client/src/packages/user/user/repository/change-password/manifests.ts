import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS = 'Umb.Repository.User.ChangePassword';

const changePasswordRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
	name: 'Change User Password Repository',
	api: () => import('./change-user-password.repository.js'),
};

export const manifests = [changePasswordRepository];
