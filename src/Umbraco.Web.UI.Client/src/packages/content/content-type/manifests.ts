import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests = [...workspaceManifests, ...modalManifests, ...conditionManifests];
