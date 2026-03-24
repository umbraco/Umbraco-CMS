import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as templateManifests } from './templates/manifests.js';
import { manifests as stylesheetManifests } from './stylesheets/manifests.js';
import { manifests as partialManifests } from './partial-views/manifests.js';
import { manifests as scriptManifest } from './scripts/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...menuManifests,
	...templateManifests,
	...stylesheetManifests,
	...partialManifests,
	...modalManifests,
	...scriptManifest,
];
