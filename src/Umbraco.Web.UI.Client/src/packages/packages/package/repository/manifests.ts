import { UmbPackageRepository } from './package.repository.js';
import { UmbPackageStore } from './package.store.js';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const PACKAGE_REPOSITORY_ALIAS = 'Umb.Repository.Package';

const repository: ManifestRepository = {
	type: 'repository',
	alias: PACKAGE_REPOSITORY_ALIAS,
	name: 'Package Repository',
	api: UmbPackageRepository,
};

export const PACKAGE_STORE_ALIAS = 'Umb.Store.Package';

const store: ManifestStore = {
	type: 'store',
	alias: PACKAGE_STORE_ALIAS,
	name: 'Package Store',
	api: UmbPackageStore,
};

export const manifests = [store, repository];
