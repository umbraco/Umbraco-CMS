import { manifests as textFilterManifests } from './text/manifests.js';
import { manifests as facetFilterManifests } from './facet-filter/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...textFilterManifests,
	...facetFilterManifests,
];
