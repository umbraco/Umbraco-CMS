import { manifests as createManifests } from './create/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...createManifests,
	...repositoryManifests,
];
