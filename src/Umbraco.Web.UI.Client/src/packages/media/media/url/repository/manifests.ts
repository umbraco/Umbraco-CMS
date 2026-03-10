import { UMB_MEDIA_URL_REPOSITORY_ALIAS, UMB_MEDIA_URL_STORE_ALIAS } from './constants.js';
import { UmbMediaUrlStore } from './media-url.store.js';
import type { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const urlRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_URL_REPOSITORY_ALIAS,
	name: 'Media Url Repository',
	api: () => import('./media-url.repository.js'),
};

const urlStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_MEDIA_URL_STORE_ALIAS,
	name: 'Media Url Store',
	api: UmbMediaUrlStore,
};

export const manifests = [urlRepository, urlStore];
