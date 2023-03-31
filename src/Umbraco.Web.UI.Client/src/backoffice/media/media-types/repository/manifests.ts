import { UmbMediaTypeRepository } from './media-type.repository';
import { UmbMediaTypeStore } from './media-type.detail.store';
import { UmbMediaTypeTreeStore } from './media-type.tree.store';
import type { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extensions-registry';

export const MEDIA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.MediaType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Media Type Repository',
	class: UmbMediaTypeRepository,
};

export const MEDIA_TYPE_STORE_ALIAS = 'Umb.Store.MediaType';
export const MEDIA_TYPE_TREE_STORE_ALIAS = 'Umb.Store.MediaTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: MEDIA_TYPE_STORE_ALIAS,
	name: 'Media Type Store',
	class: UmbMediaTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEDIA_TYPE_TREE_STORE_ALIAS,
	name: 'Media Type Tree Store',
	class: UmbMediaTypeTreeStore,
};

export const manifests = [store, treeStore, repository];
