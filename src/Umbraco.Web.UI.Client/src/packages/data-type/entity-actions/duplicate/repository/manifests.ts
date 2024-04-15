import { UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { UmbDuplicateDataTypeRepository } from './data-type-duplicate.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const duplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Duplicate Data Type Repository',
	api: UmbDuplicateDataTypeRepository,
};

export const manifests = [duplicateRepository];
