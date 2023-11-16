import { UmbPartialViewRepository } from './partial-view.repository.js';
import { UmbPartialViewStore } from './partial-view.store.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PARTIAL_VIEW_REPOSITORY_ALIAS = 'Umb.Repository.PartialView';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
	name: 'Partial View Repository',
	api: UmbPartialViewRepository,
};

export const UMB_PARTIAL_VIEW_STORE_ALIAS = 'Umb.Store.PartialView';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_PARTIAL_VIEW_STORE_ALIAS,
	name: 'Partial View Store',
	api: UmbPartialViewStore,
};

export const manifests = [repository, store];
