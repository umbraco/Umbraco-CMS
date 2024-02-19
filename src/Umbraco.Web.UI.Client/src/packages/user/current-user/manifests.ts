import { UmbCurrentUserContext } from './current-user.context.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as userProfileAppsManifests } from './user-profile-apps/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const headerApps: Array<ManifestTypes> = [
	{
		type: 'store',
		alias: 'Umb.Store.CurrentUser.History',
		name: 'Current User History Store',
		api: () => import('./current-user-history.store.js'),
	},
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.CurrentUser',
		name: 'Current User',
		api: UmbCurrentUserContext,
	},
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.CurrentUser',
		name: 'Current User',
		element: () => import('./current-user-header-app.element.js'),
		weight: 0,
		meta: {
			label: 'TODO: how should we enable this to not be set.',
			icon: 'TODO: how should we enable this to not be set.',
			pathname: 'user',
		},
	},
];

export const manifests = [...headerApps, ...modalManifests, ...repositoryManifests, ...userProfileAppsManifests];
