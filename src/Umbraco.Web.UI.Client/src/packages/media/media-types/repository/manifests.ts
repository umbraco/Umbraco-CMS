import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';

export const manifests = [...detailManifests, ...itemManifests, ...folderManifests];
