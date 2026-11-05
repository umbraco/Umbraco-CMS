import { manifests as actionGroupManifests } from './action-groups/manifests.js';
import { manifests as kindManifests } from './kinds/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...actionGroupManifests,
	...kindManifests,
];
