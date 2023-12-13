import { UmbScriptDetailRepository } from './script-detail.repository.js';
import { UmbScriptDetailStore } from './script-detail.store.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS = 'Umb.Repository.Script.Detail';
export const UMB_SCRIPT_DETAIL_STORE_ALIAS = 'Umb.Store.Script.Detail';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
	name: 'Script Detail Repository',
	api: UmbScriptDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_SCRIPT_DETAIL_STORE_ALIAS,
	name: 'Script Detail Store',
	api: UmbScriptDetailStore,
};

export const manifests = [repository, store];
