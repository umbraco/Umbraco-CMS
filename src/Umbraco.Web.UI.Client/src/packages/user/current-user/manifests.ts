import { manifest as actionDefaultKindManifest } from './action/default.kind.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as externalLoginProviderManifests } from './external-login/manifests.js';
import { manifests as historyManifests } from './history/manifests.js';
import { manifests as mfaLoginProviderManifests } from './mfa-login/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as profileManifests } from './profile/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as themeManifests } from './theme/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
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
	},
	actionDefaultKindManifest,
	...conditionManifests,
	...externalLoginProviderManifests,
	...historyManifests,
	...mfaLoginProviderManifests,
	...modalManifests,
	...profileManifests,
	...repositoryManifests,
	...themeManifests,
];
