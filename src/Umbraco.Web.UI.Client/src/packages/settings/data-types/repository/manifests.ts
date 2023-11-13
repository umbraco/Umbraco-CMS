import { UmbDataTypeDetailRepository } from './detail/data-type-detail.repository.js';
import { UmbDataTypeStore } from './data-type.store.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as moveManifests } from './move/manifests.js';
import { manifests as copyManifests } from './copy/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Data Type Repository',
	api: UmbDataTypeDetailRepository,
};

export const DATA_TYPE_STORE_ALIAS = 'Umb.Store.DataType';

const store: ManifestStore = {
	type: 'store',
	alias: DATA_TYPE_STORE_ALIAS,
	name: 'Data Type Store',
	api: UmbDataTypeStore,
};

export const manifests = [repository, store, ...itemManifests, ...moveManifests, ...copyManifests, ...folderManifests];
