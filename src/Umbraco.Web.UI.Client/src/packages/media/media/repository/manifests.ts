import { UmbMediaRepository } from './media.repository.js';
import { UmbMediaStore } from './media.store.js';
import { UmbMediaTreeStore } from './media.tree.store.js';
import type { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const MEDIA_REPOSITORY_ALIAS = 'Umb.Repository.Media';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEDIA_REPOSITORY_ALIAS,
	name: 'Media Repository',
	api: UmbMediaRepository,
};

export const MEDIA_STORE_ALIAS = 'Umb.Store.Media';
export const MEDIA_TREE_STORE_ALIAS = 'Umb.Store.MediaTree';

const store: ManifestStore = {
	type: 'store',
	alias: MEDIA_STORE_ALIAS,
	name: 'Media Store',
	api: UmbMediaStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: MEDIA_TREE_STORE_ALIAS,
	name: 'Media Tree Store',
	api: UmbMediaTreeStore,
};

export const manifests = [store, treeStore, repository];
