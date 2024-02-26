import { manifests as blockManifests } from './block/manifests.js';
import { manifests as blockGridManifests } from './block-grid/manifests.js';
import { manifests as blockListManifests } from './block-list/manifests.js';
import { manifests as blockRteManifests } from './block-rte/manifests.js';
import { manifests as blockTypeManifests } from './block-type/manifests.js';

export const manifests = [
	...blockManifests,
	...blockTypeManifests,
	...blockListManifests,
	...blockGridManifests,
	...blockRteManifests,
];
