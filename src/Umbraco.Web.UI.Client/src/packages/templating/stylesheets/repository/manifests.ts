import { UmbStylesheetDetailRepository } from './stylesheet-detail.repository.js';
import { manifests as itemManifests } from './item/manifests.js';
import { UmbStylesheetDetailStore } from './stylesheet-detail.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Stylesheet.Detail';
export const UMB_STYLESHEET_DETAIL_STORE_ALIAS = 'Umb.Store.Stylesheet.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS,
	name: 'Stylesheet Detail Repository',
	api: UmbStylesheetDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_STYLESHEET_DETAIL_STORE_ALIAS,
	name: 'Stylesheet Detail Store',
	api: UmbStylesheetDetailStore,
};

export const manifests = [repository, store, ...itemManifests];
