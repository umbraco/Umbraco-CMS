import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';

export const manifests = [...repositoryManifests, ...menuItemManifests, ...workspaceManifests, ...collectionManifests];
