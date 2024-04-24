import { UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const duplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Duplicate Data Type Repository',
	api: () => import('./data-type-duplicate.repository.js'),
};

export const manifests: Array<ManifestTypes> = [duplicateRepository];
