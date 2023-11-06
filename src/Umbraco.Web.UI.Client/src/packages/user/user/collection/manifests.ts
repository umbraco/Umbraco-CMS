import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import { manifests as collectionViewManifests } from './views/manifests.js';

export const manifests = [...collectionRepositoryManifests, ...collectionViewManifests];
