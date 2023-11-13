import { UmbCopyDataTypeRepository } from './data-type-copy.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const COPY_DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Copy';

const copyRepository: ManifestRepository = {
	type: 'repository',
	alias: COPY_DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Copy Data Type Repository',
	api: UmbCopyDataTypeRepository,
};

export const manifests = [copyRepository];
