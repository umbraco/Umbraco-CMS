import { UMB_PACKAGE_REPOSITORY_ALIAS, UMB_PACKAGE_STORE_ALIAS } from './constants.js';
import type { ManifestStore, ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PACKAGE_REPOSITORY_ALIAS,
	name: 'Package Repository',
	api: () => import('./package.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: UMB_PACKAGE_STORE_ALIAS,
	name: 'Package Store',
	api: () => import('./package.store.js'),
};

export const manifests: Array<ManifestTypes> = [store, repository];
