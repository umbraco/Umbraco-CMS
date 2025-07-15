import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as contentTreeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests = [
	...collectionManifests,
	...conditionManifests,
	...contentTreeManifests,
	...workspaceManifests,
];
