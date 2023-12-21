import { manifests as blockTypeManifests } from './block-type/manifests.js';
import { manifests as blockManifests } from './block/manifests.js';

export const manifests = [...blockTypeManifests, ...blockManifests];
