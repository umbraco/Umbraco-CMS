import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';
import { manifests as modalManifests } from './modals/manifests';
import { manifests as userProfileAppsManifests } from './user-profile-apps/manifests';

export const headerApps: Array<ManifestTypes> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.CurrentUser',
		name: 'Current User',
		loader: () => import('./current-user-header-app.element'),
		weight: 1000,
		meta: {
			label: 'TODO: how should we enable this to not be set.',
			icon: 'TODO: how should we enable this to not be set.',
			pathname: 'user',
		},
	},
];

export const manifests = [...headerApps, ...modalManifests, ...userProfileAppsManifests];
