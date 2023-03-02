import { UmbMediaRepository } from './media.repository';
import { UmbMediaStore } from './media.store';
import { UmbMediaTreeStore } from './media.tree.store';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';
import { ManifestStore, ManifestTreeStore } from '@umbraco-cms/extensions-registry';

export const MEDIA_REPOSITORY_ALIAS = 'Umb.Repository.Media';

const repository: ManifestRepository = {
	type: 'repository',
	alias: MEDIA_REPOSITORY_ALIAS,
	name: 'Media Repository',
	class: UmbMediaRepository,
};

export const MEDIA_STORE_ALIAS = 'Umb.Store.Media';
export const MEDIA_TREE_STORE_ALIAS = 'Umb.Store.MediaTree';

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

export const manifests = [store, treeStore, repository];
