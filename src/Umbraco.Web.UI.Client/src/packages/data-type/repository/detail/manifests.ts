import { UmbDataTypeDetailRepository } from './data-type-detail.repository.js';
import { UmbDataTypeDetailStore } from './data-type-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Data Type Detail Repository',
	api: UmbDataTypeDetailRepository,
};

export const UMB_DATA_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.DataType.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DATA_TYPE_DETAIL_STORE_ALIAS,
	name: 'Data Type Detail Store',
	api: UmbDataTypeDetailStore,
};

export const manifests = [repository, store];
