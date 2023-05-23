import { UmbDataTypeRepository } from './data-type.repository';
import { UmbDataTypeItemStore } from './data-type-item.store';
import { UmbDataTypeStore } from './data-type.store';
import { UmbDataTypeTreeStore } from './data-type.tree.store';
import type {
	ManifestStore,
	ManifestTreeStore,
	ManifestRepository,
	ManifestItemStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Data Type Repository',
	class: UmbDataTypeRepository,
};

export const DATA_TYPE_STORE_ALIAS = 'Umb.Store.DataType';
export const DATA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DataTypeTree';
export const DATA_TYPE_ITEM_STORE_ALIAS = 'Umb.Store.DataTypeItem';

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

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: DATA_TYPE_ITEM_STORE_ALIAS,
	name: 'Data Type Item Store',
	class: UmbDataTypeItemStore,
};

export const manifests = [repository, store, treeStore, itemStore];
