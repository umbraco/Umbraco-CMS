import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_DETAIL_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

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

export const manifests: Array<ManifestTypes> = [detailRepository, detailStore];
