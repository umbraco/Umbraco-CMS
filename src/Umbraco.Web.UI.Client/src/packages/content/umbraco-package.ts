import { manifests as contentManifests } from './content/manifests.js';
import { manifests as contentTypeManifests } from './content-type/manifests.js';
import { manifests as propertyTypeManifests } from './property-type/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...contentManifests,
	...contentTypeManifests,
	...propertyTypeManifests,
];

export const name = 'Umbraco.Content';
export const extensions = [
	{
		name: 'Content Bundle',
		alias: 'Umb.Bundle.Content',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
