import { UmbDataTypeRepository } from '../repository/data-type.repository';
import { UmbDataTypeStore } from './data-type.store';
import { UmbDataTypeTreeStore } from './data-type.tree.store';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';
import { ManifestStore, ManifestTreeStore } from '@umbraco-cms/extensions-registry';

export const DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Data Type Repository',
	class: UmbDataTypeRepository,
};

export const DATA_TYPE_STORE_ALIAS = 'Umb.Store.DataType';
export const DATA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DataTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: DATA_TYPE_STORE_ALIAS,
	name: 'Data Type Store',
	class: UmbDataTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DATA_TYPE_TREE_STORE_ALIAS,
	name: 'Data Type Tree Store',
	class: UmbDataTypeTreeStore,
};

export const manifests = [repository, store, treeStore];
