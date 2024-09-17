import { manifests as headerAppManifests } from './header-app/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...menuManifests,
	...headerAppManifests,
];
