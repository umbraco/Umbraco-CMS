import { UMB_DISABLE_USER_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const disableRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DISABLE_USER_REPOSITORY_ALIAS,
	name: 'Disable User Repository',
	api: () => import('./disable-user.repository.js'),
};

export const manifests: Array<ManifestTypes> = [disableRepository];
