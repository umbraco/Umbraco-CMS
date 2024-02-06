import { UmbDisableUserRepository } from './disable-user.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DISABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Disable';
const disableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DISABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbDisableUserRepository,
};

export const manifests = [disableRepository];
