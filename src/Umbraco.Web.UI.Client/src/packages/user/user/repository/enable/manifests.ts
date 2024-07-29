import { UMB_ENABLE_USER_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const enableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_ENABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: () => import('./enable-user.repository.js'),
};

export const manifests: Array<ManifestTypes> = [enableRepository];
