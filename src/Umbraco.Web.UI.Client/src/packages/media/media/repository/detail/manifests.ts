import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS, UMB_MEDIA_DETAIL_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
	name: 'Media Detail Repository',
	api: () => import('./media-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_MEDIA_DETAIL_STORE_ALIAS,
	name: 'Media Detail Store',
	api: () => import('./media-detail.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
