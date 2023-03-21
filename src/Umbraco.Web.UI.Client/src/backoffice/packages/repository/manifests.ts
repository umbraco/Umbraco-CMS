import { UmbPackageRepository } from './package.repository';
import { UmbPackageStore } from './package.store';
import { ManifestRepository } from 'libs/extensions-registry/repository.models';
import { ManifestStore } from '@umbraco-cms/extensions-registry';

export const PACKAGE_REPOSITORY_ALIAS = 'Umb.Repository.Package';

const repository: ManifestRepository = {
	type: 'repository',
	alias: PACKAGE_REPOSITORY_ALIAS,
	name: 'Package Repository',
	class: UmbPackageRepository,
};

export const PACKAGE_STORE_ALIAS = 'Umb.Store.Package';

const store: ManifestStore = {
	type: 'store',
	alias: PACKAGE_STORE_ALIAS,
	name: 'Package Store',
	class: UmbPackageStore,
};

export const manifests = [store, repository];
