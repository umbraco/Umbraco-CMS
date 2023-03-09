import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';

export const manifests = [...repositoryManifests, ...workspaceManifests];
