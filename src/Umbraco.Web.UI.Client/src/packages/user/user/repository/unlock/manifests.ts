import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_UNLOCK_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Unlock';

const unlockRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_UNLOCK_USER_REPOSITORY_ALIAS,
	name: 'Unlock User Repository',
	api: () => import('./unlock-user.repository.js'),
};

export const manifests = [unlockRepository];
