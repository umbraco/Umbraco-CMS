import { UmbDataTypeRepository } from './data-type.repository.js';
import { UmbDataTypeStore } from './data-type.store.js';
import { UmbDataTypeTreeStore } from './data-type.tree.store.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as moveManifests } from './move/manifests.js';
import { manifests as copyManifests } from './copy/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import type { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Data Type Repository',
	api: UmbDataTypeRepository,
};

export const DATA_TYPE_STORE_ALIAS = 'Umb.Store.DataType';
export const DATA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DataTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: DATA_TYPE_STORE_ALIAS,
	name: 'Data Type Store',
	api: UmbDataTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DATA_TYPE_TREE_STORE_ALIAS,
	name: 'Data Type Tree Store',
	api: UmbDataTypeTreeStore,
};

export const manifests = [
	repository,
	store,
	treeStore,
	...itemManifests,
	...moveManifests,
	...copyManifests,
	...folderManifests,
];
