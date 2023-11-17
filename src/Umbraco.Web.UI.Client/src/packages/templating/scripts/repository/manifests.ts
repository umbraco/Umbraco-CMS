import { UmbScriptRepository } from './script.repository.js';
import { UmbScriptStore } from './script.store.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SCRIPT_REPOSITORY_ALIAS = 'Umb.Repository.Script';
export const UMB_SCRIPT_STORE_ALIAS = 'Umb.Store.Script';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SCRIPT_REPOSITORY_ALIAS,
	name: 'Script Repository',
	api: UmbScriptRepository,
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_SCRIPT_STORE_ALIAS,
	name: 'Script Store',
	api: UmbScriptStore,
};

export const manifests = [repository, store, ...folderManifests];
