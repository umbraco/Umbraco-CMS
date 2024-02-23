import type { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_PACKAGE_REPOSITORY_ALIAS = 'Umb.Repository.Package';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_PACKAGE_REPOSITORY_ALIAS,
	name: 'Package Repository',
	api: () => import('./package.repository.js'),
};

export const UMB_PACKAGE_STORE_ALIAS = 'Umb.Store.Package';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_PACKAGE_STORE_ALIAS,
	name: 'Package Store',
	api: () => import('./package.store.js'),
};

export const manifests = [store, repository];
