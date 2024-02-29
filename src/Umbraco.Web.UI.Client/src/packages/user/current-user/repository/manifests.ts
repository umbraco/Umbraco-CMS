import { UmbCurrentUserStore } from './current-user.store.js';
import type { ManifestRepository, ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CURRENT_USER_REPOSITORY_ALIAS = 'Umb.Repository.CurrentUser';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_CURRENT_USER_REPOSITORY_ALIAS,
	name: 'Current User Repository',
	api: () => import('./current-user.repository.js'),
};

const store: ManifestStore = {
	type: 'store',
	alias: 'Umb.Store.CurrentUser',
	name: 'Current User Store',
	api: UmbCurrentUserStore,
};

export const manifests = [repository, store];
