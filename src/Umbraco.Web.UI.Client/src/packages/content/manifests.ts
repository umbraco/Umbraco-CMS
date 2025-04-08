import { manifests as contentManifests } from './content/manifests.js';
import { manifests as contentTypeManifests } from './content-type/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...contentManifests,
	...contentTypeManifests,
];
