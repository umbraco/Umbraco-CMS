import { UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_GROUP_COLLECTION_REPOSITORY_ALIAS,
	name: 'User Group Collection Repository',
	api: () => import('./user-group-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
