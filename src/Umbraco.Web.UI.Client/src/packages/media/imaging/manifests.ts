import { UMB_IMAGING_REPOSITORY_ALIAS, UMB_IMAGING_STORE_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestStore, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_IMAGING_REPOSITORY_ALIAS,
	name: 'Imaging Repository',
	api: () => import('./imaging.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_IMAGING_STORE_ALIAS,
	name: 'Imaging Store',
	api: () => import('./imaging.store.js'),
};

export const manifests: Array<ManifestTypes> = [repository, store];
