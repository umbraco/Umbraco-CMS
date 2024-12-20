import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as contextManifests } from './context/manifests.js';
import { manifests as currentUserActionManifests } from './current-user-action/manifests.js';
import { manifests as entryManifests } from './clipboard-entry/manifests.js';
import { manifests as propertyActionManifests } from './property-actions/manifests.js';
import { manifests as rootManifests } from './clipboard-root/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...contextManifests,
	...currentUserActionManifests,
	...entryManifests,
	...propertyActionManifests,
	...rootManifests,
];
