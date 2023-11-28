import { manifests as collectionActionManifests } from './collection-action/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';

export const manifests = [
	...collectionActionManifests,
	...modalManifests,
	...repositoryManifests,
	...entityActionManifests,
];
