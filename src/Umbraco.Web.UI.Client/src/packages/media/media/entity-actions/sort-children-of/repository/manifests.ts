import { UMB_SORT_CHILDREN_OF_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SORT_CHILDREN_OF_MEDIA_REPOSITORY_ALIAS,
	name: 'Sort Children Of Media Repository',
	api: () => import('./sort-children-of.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
