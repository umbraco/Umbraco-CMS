import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as sectionViewManifests } from './section-view/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';

export const manifests = [
	...collectionManifests,
	...repositoryManifests,
	...workspaceManifests,
	...modalManifests,
	...sectionViewManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
];
