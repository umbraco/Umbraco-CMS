import { UmbMediaTypeRepository } from './media-type.repository.js';
import { UmbMediaTypeStore } from './media-type.detail.store.js';
import { UmbMediaTypeTreeStore } from './media-type.tree.store.js';
import type { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const MEDIA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.MediaType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Media Type Repository',
	api: UmbMediaTypeRepository,
};

export const MEDIA_TYPE_STORE_ALIAS = 'Umb.Store.MediaType';
export const MEDIA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.MediaTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: MEDIA_TYPE_STORE_ALIAS,
	name: 'Media Type Store',
	api: UmbMediaTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEDIA_TYPE_TREE_STORE_ALIAS,
	name: 'Media Type Tree Store',
	api: UmbMediaTypeTreeStore,
};

export const manifests = [store, treeStore, repository];
