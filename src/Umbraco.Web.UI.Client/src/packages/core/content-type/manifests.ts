import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';

export const manifests = [...workspaceManifests, ...modalManifests];
