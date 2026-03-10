import { manifests as dataManifests } from './data/manifests.js';
import { manifests as refManifests } from './ref/manifests.js';

export const manifests = [...dataManifests, ...refManifests];
