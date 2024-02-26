import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as externalLoginProviderManifests } from './external-login/manifests.js';
import { manifests as historyManifests } from './history/manifests.js';
import { manifests as profileManifests } from './profile/manifests.js';
import { manifests as themeManifests } from './theme/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const headerApps: Array<ManifestTypes> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.CurrentUser',
		name: 'Current User',
		api: () => import('./current-user.context.js'),
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

export const manifests = [
	...externalLoginProviderManifests,
	...headerApps,
	...historyManifests,
	...modalManifests,
	...profileManifests,
	...repositoryManifests,
	...themeManifests,
];
