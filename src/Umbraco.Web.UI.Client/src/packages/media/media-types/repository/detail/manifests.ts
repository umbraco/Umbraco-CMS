import { UmbMediaTypeDetailRepository } from './media-type-detail.repository.js';
import { UmbMediaTypeDetailStore } from './media-type-detail.store.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.MediaType.Detail';
export const UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS = 'Umb.Store.MediaType.Detail';

const detailRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
	name: 'Media Types Repository',
	api: UmbMediaTypeDetailRepository,
};

const detailStore: ManifestStore = {
	type: 'store',
	alias: UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS,
	name: 'Media Type Store',
	api: UmbMediaTypeDetailStore,
};

export const manifests = [detailRepository, detailStore];
