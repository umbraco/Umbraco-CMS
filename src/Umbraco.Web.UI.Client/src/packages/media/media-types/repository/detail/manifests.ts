import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Detail';
export const UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.MediaType.Detail';

const detailRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Media Types Repository',
	api: () => import('./media-type-detail.repository.js'),
};

const detailStore: ManifestStore = {
	type: 'store',
	alias: UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS,
	name: 'Media Type Store',
	api: () => import('./media-type-detail.store.js'),
};

export const manifests = [detailRepository, detailStore];
