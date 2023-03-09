import { UmbUserRepository } from '../repository/user.repository';
import { UmbUserStore } from './user.store';
import type { ManifestStore, ManifestRepository } from '@umbraco-cms/extensions-registry';

export const USER_REPOSITORY_ALIAS = 'Umb.Repository.User';

const repository: ManifestRepository = {
	type: 'repository',
	alias: USER_REPOSITORY_ALIAS,
	name: 'User Repository',
	class: UmbUserRepository,
};

export const USER_STORE_ALIAS = 'Umb.Store.User';

const store: ManifestStore = {
	type: 'store',
	alias: USER_STORE_ALIAS,
	name: 'User Store',
	class: UmbUserStore,
};

export const manifests = [repository, store];
