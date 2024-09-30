// eslint-disable-next-line local-rules/no-relative-import-to-import-map-module
import { manifests as tiptapManifests } from './tiptap/manifests.js';
import { manifests as tinyMceManifests } from './tiny-mce/manifests.js';
import { manifest as schemaManifest } from './Umbraco.RichText.js';
import { manifest as blockRtePropertyValueResolver } from './property-value-resolver/manifest.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	...tinyMceManifests,
	...tiptapManifests,
	schemaManifest,
	blockRtePropertyValueResolver,
];
