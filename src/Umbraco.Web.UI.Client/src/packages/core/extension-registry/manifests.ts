import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifest as modalManifest } from './modals/manifest-viewer/manifest.js';
import type { ManifestTypes } from './models/index.js';

export const manifests: Array<ManifestTypes> = [
	...conditionManifests,
	...menuItemManifests,
	...workspaceManifests,
	...collectionManifests,
	...entityActionManifests,
	modalManifest,
];
