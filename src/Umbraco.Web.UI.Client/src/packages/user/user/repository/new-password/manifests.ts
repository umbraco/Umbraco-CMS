import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_NEW_USER_PASSWORD_REPOSITORY_ALIAS = 'Umb.Repository.User.NewPassword';

const newPasswordRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_NEW_USER_PASSWORD_REPOSITORY_ALIAS,
	name: 'New User Password Repository',
	api: () => import('./new-user-password.repository.js'),
};

export const manifests = [newPasswordRepository];
