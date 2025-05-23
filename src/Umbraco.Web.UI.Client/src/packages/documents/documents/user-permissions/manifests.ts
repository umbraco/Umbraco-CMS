import { manifests as documentManifests } from './document/manifests.js';
import { manifests as documentPropertyValueManifests } from './document-property-value/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...documentManifests,
	...documentPropertyValueManifests,
];
