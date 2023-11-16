import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';

export const manifests = [...treeManifests, ...menuItemManifests, ...entityActionManifests];
