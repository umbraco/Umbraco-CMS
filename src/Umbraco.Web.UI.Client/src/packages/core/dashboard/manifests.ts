import type { UmbExtensionManifestKind } from '../extension-registry/registry.js';
import { manifests as defaultManifests } from './default/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...defaultManifests,
	...conditionManifests,
];
