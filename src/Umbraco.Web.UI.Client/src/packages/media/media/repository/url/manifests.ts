import type { ManifestItemStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_URL_REPOSITORY_ALIAS = 'Umb.Repository.Media.Url';
export const UMB_MEDIA_URL_STORE_ALIAS = 'Umb.Store.MediaUrl';

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
	api: () => import('./media-url.store.js'),
};

export const manifests = [urlRepository, urlStore];
