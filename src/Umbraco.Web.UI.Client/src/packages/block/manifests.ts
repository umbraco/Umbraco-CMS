import { manifests as blockManifests } from './block/manifests.js';
import { manifests as blockGridManifests } from './block-grid/manifests.js';
import { manifests as blockListManifests } from './block-list/manifests.js';
import { manifests as blockRteManifests } from './block-rte/manifests.js';
import { manifests as blockTypeManifests } from './block-type/manifests.js';
import { manifest as modalManifest } from './modals/manifest-viewer/manifest.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
// TODO: Remove test custom view, or transfer to test or similar?
//import { manifest } from './custom-view/manifest.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	//manifest,
	...blockManifests,
	...blockTypeManifests,
	...blockListManifests,
	...blockGridManifests,
	...blockRteManifests,
	modalManifest,
];
