import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as contextManifests } from './context/manifests.js';
import { manifests as entryManifests } from './clipboard-entry/manifests.js';
import { manifests as propertyManifests } from './property/manifests.js';
import { manifests as rootManifests } from './clipboard-root/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...contextManifests,
	...entryManifests,
	...propertyManifests,
	...rootManifests,
];

export const name = 'Umbraco.Clipboard';
export const extensions = [
	{
		name: 'Clipboard Bundle',
		alias: 'Umb.Bundle.Clipboard',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
