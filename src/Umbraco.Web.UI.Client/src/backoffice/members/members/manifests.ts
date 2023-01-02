import { manifests as treeManifests } from './tree/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';

export const manifests = [treeManifests, ...workspaceManifests];
