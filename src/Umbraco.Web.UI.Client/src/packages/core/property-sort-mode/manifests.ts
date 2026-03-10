import { manifests as propertyActionManifests } from './property-action/manifests.js';
import { manifests as propertyContextManifests } from './property-context/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...propertyActionManifests,
	...propertyContextManifests,
];
