import { manifests as facetFilterActiveValueManifests } from './active-value/manifests.js';
import { manifests as selectManifests } from './select/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...facetFilterActiveValueManifests,
	...selectManifests,
];
