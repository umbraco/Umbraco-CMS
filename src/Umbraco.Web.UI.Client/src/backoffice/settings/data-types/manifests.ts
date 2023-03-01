import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as menuItemManifests } from './menu-item/manifests';
import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';

export const manifests = [...repositoryManifests, ...menuItemManifests, ...treeManifests, ...workspaceManifests];
