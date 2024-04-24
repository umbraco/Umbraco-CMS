import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_COLLECTION_REPOSITORY_ALIAS,
	name: 'Data Type Collection Repository',
	api: () => import('./data-type-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
