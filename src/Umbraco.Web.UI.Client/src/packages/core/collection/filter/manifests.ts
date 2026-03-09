import { manifests as textFilterManifests } from './text/manifests.js';
import { manifests as selectFilterManifests } from './select/manifests.js';
import { manifests as multiSelectFilterManifests } from './multi-select/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...textFilterManifests,
	...selectFilterManifests,
	...multiSelectFilterManifests,
];
