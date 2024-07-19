import { manifests as blockManifests } from './block/manifests.js';
import { manifests as blockGridManifests } from './block-grid/manifests.js';
import { manifests as blockListManifests } from './block-list/manifests.js';
import { manifests as blockRteManifests } from './block-rte/manifests.js';
import { manifests as blockTypeManifests } from './block-type/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
// TODO: Remove test custom view, or transfer to test or similar?
//import { manifest } from './custom-view/manifest.js';

export const manifests: Array<ManifestTypes> = [
	//manifest,
	...blockManifests,
	...blockTypeManifests,
	...blockListManifests,
	...blockGridManifests,
	...blockRteManifests,
];
