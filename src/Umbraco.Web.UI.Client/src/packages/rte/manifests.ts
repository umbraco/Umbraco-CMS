import { manifest as schemaManifest } from './Umbraco.RichText.js';
import { manifest as blockRtePropertyValueResolver } from './property-value-resolver/manifest.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	schemaManifest,
	blockRtePropertyValueResolver,
];
