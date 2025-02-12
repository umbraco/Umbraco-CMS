import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';

export const manifests = [...workspaceManifests, ...conditionManifests];
