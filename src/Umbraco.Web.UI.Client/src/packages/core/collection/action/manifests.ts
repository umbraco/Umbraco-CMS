import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as buttonManifests } from './button/manifests.js';
import { manifests as createManifests } from './create/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...buttonManifests,
	...createManifests,
];
