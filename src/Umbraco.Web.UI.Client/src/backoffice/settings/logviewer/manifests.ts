import { manifests as treeManifests } from './menu-item/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';

export const manifests = [...treeManifests, ...workspaceManifests];
