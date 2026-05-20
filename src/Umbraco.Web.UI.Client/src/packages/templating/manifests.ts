// Side-effect imports: register the global components for each templating
// sub-feature at module load. Replaces the side effect previously performed by
// the package's `backofficeEntryPoint` extension when it was loaded via
// CORE_PACKAGES.
import './partial-views/global-components/index.js';
import './scripts/global-components/index.js';
import './stylesheets/global-components/index.js';
import './templates/global-components/index.js';
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
