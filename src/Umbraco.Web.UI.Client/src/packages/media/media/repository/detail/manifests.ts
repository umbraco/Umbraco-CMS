import { UmbMediaDetailRepository } from './media-detail.repository.js';
import { UmbMediaDetailStore } from './media-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Media.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
	name: 'Media Detail Repository',
	api: UmbMediaDetailRepository,
};

export const UMB_MEDIA_DETAIL_STORE_ALIAS = 'Umb.Store.Media.Detail';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEDIA_DETAIL_STORE_ALIAS,
	name: 'Media Detail Store',
	api: UmbMediaDetailStore,
};

export const manifests = [repository, store];
