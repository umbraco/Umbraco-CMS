import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_INVITE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Invite';
const inviteRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_INVITE_USER_REPOSITORY_ALIAS,
	name: 'Invite User Repository',
	api: () => import('./invite-user.repository.js'),
};

export const manifests = [inviteRepository];
