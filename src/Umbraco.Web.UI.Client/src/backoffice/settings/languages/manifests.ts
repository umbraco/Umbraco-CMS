import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as treeManifests } from './sidebar-menu-item/manifests';
import { manifests as entityActions } from './entity-actions/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';

export const manifests = [...repositoryManifests, ...entityActions, ...treeManifests, ...workspaceManifests];
