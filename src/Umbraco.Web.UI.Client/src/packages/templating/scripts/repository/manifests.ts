import { UmbScriptDetailRepository } from './script-detail.repository.js';
import { UmbScriptDetailStore } from './script-detail.store.js';
import { manifests as folderManifests } from '../tree/folder/manifests.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_REPOSITORY_ALIAS = 'Umb.Repository.Script';
export const UMB_SCRIPT_STORE_ALIAS = 'Umb.Store.Script';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_REPOSITORY_ALIAS,
	name: 'Script Repository',
	api: UmbScriptDetailRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_SCRIPT_STORE_ALIAS,
	name: 'Script Store',
	api: UmbScriptDetailStore,
};

export const manifests = [repository, store, ...folderManifests];
