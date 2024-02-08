import { UmbCurrentUserRepository } from './current-user.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CURRENT_USER_REPOSITORY_ALIAS = 'Umb.Repository.CurrentUser';

const avatarRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_CURRENT_USER_REPOSITORY_ALIAS,
	name: 'Current User Repository',
	api: UmbCurrentUserRepository,
};

export const manifests = [avatarRepository];
