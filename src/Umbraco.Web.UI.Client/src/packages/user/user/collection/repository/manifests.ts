import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_USER_COLLECTION_REPOSITORY_ALIAS,
	name: 'User Collection Repository',
	api: () => import('./user-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
