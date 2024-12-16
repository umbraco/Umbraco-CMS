import type { UmbExtensionManifestKind } from '../extension-registry/registry.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entryManifests } from './clipboard-entry/manifests.js';
import { manifests as propertyActionManifests } from './property-actions/manifests.js';
import { manifests as rootManifests } from './clipboard-root/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...entryManifests,
	...propertyActionManifests,
	...rootManifests,
];
