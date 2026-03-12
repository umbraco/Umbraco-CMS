import { manifests as selectManifests } from './select/manifests.js';
import { manifests as multiSelectManifests } from './multi-select/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...selectManifests,
	...multiSelectManifests,
];
