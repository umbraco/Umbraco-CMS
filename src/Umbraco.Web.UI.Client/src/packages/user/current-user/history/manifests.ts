import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const userProfileApps: Array<ManifestTypes> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.CurrentUser.History',
		name: 'Current User History User Profile App',
		element: () => import('../history/current-user-history-user-profile-app.element.js'),
		weight: 100,
		meta: {
			label: 'History',
			pathname: 'history',
		},
	},
	{
		type: 'store',
		alias: 'Umb.Store.CurrentUser.History',
		name: 'Current User History Store',
		api: () => import('./current-user-history.store.js'),
	},
];
export const manifests = [...userProfileApps];
