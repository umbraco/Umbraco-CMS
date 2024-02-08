import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests = [
	...modalManifests,
	...repositoryManifests,
];
