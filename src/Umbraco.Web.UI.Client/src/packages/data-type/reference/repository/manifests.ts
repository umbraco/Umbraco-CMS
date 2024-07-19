import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Reference';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS,
	name: 'Data Type Reference Repository',
	api: () => import('./data-type-reference.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
