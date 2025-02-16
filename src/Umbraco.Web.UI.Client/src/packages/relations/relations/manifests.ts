import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as trashManifests } from './entity-actions/trash/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...trashManifests,
];
