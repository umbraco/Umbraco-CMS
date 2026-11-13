import { manifests as kindManifests } from './kinds/manifests.js';
import { manifests as textFilterManifests } from './text/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...kindManifests,
	...textFilterManifests,
];
