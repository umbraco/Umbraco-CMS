import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests = [...repositoryManifests, ...entityActionManifests, ...workspaceManifests];
