import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';

export const manifests = [...repositoryManifests, ...treeManifests, ...menuItemManifests];
