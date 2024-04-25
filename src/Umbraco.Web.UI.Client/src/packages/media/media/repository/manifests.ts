import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as urlManifests } from './url/manifests.js';

export const manifests = [...detailManifests, ...itemManifests, ...urlManifests];
