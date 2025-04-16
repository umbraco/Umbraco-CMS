import { manifest as blockRtePropertyValueResolver } from './property-value-resolver/manifest.js';
import { manifest as schemaManifest } from './Umbraco.RichText.js';
import { manifests as validationManifests } from './validation/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...validationManifests,
	blockRtePropertyValueResolver,
	schemaManifest,
];
