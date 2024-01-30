import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as publishingManifests } from './publishing/manifests.js';

export const manifests = [...detailManifests, ...itemManifests, ...publishingManifests];
