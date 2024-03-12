import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests = [...menuItemManifests, ...workspaceManifests, ...entityActionManifests, ...treeManifests];
