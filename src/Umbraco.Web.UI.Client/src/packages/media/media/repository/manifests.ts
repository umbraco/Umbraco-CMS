import { UmbMediaItemStore } from './media-item.store.js';
import { UmbMediaRepository } from './media.repository.js';
import { UmbMediaStore } from './media.store.js';
import type { ManifestStore, ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_REPOSITORY_ALIAS = 'Umb.Repository.Media';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_REPOSITORY_ALIAS,
	name: 'Media Repository',
	api: UmbMediaRepository,
};

export const UMB_MEDIA_STORE_ALIAS = 'Umb.Store.Media';
export const UMB_MEDIA_ITEM_STORE_ALIAS = 'Umb.Store.MediaItem';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEDIA_STORE_ALIAS,
	name: 'Media Store',
	api: UmbMediaStore,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEDIA_ITEM_STORE_ALIAS,
	name: 'Media Item Store',
	api: UmbMediaItemStore,
};

export const manifests = [store, itemStore, repository];
