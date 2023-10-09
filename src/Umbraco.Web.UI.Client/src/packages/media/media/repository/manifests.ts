import { UmbMediaItemStore } from './media-item.store.js';
import { UmbMediaRepository } from './media.repository.js';
import { UmbMediaStore } from './media.store.js';
import { UmbMediaTreeStore } from './media.tree.store.js';
import type {
	ManifestStore,
	ManifestTreeStore,
	ManifestRepository,
	ManifestItemStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const MEDIA_REPOSITORY_ALIAS = 'Umb.Repository.Media';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEDIA_REPOSITORY_ALIAS,
	name: 'Media Repository',
	class: UmbMediaRepository,
};

export const MEDIA_STORE_ALIAS = 'Umb.Store.Media';
export const MEDIA_TREE_STORE_ALIAS = 'Umb.Store.MediaTree';
export const MEDIA_ITEM_STORE_ALIAS = 'Umb.Store.MediaItem';

const store: ManifestStore = {
	type: 'store',
	alias: MEDIA_STORE_ALIAS,
	name: 'Media Store',
	class: UmbMediaStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEDIA_TREE_STORE_ALIAS,
	name: 'Media Tree Store',
	class: UmbMediaTreeStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: MEDIA_ITEM_STORE_ALIAS,
	name: 'Media Item Store',
	class: UmbMediaItemStore,
};

export const manifests = [store, treeStore, itemStore, repository];
