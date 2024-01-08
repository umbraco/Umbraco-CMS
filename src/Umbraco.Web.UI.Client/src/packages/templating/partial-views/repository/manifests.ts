import { UmbPartialViewDetailRepository } from './partial-view-detail.repository.js';
import { UmbPartialViewDetailStore } from './partial-view-detail.store.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialView.Detail';
export const UMB_PARTIAL_VIEW_DETAIL_STORE_ALIAS = 'Umb.Store.PartialView.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
	name: 'Partial View Detail Repository',
	api: UmbPartialViewDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_PARTIAL_VIEW_DETAIL_STORE_ALIAS,
	name: 'Partial View Detail Store',
	api: UmbPartialViewDetailStore,
};

export const manifests = [repository, store];
