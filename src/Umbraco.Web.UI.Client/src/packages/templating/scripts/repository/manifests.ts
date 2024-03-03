import { manifests as itemManifests } from './item/manifests.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Script.Detail';
export const UMB_SCRIPT_DETAIL_STORE_ALIAS = 'Umb.Store.Script.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
	name: 'Script Detail Repository',
	api: () => import('./script-detail.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_SCRIPT_DETAIL_STORE_ALIAS,
	name: 'Script Detail Store',
	api: () => import('./script-detail.store.js'),
};

export const manifests = [repository, store, ...itemManifests];
