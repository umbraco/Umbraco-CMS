import { UmbEnableUserRepository } from './enable-user.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENABLE_USER_REPOSITORY_ALIAS = 'Umb.Repository.User.Enable';
const enableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ENABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: UmbEnableUserRepository,
};

export const manifests = [enableRepository];
