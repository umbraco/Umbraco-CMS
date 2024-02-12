import { manifests as conditionManifests } from './conditions/block-workspace-has-settings.condition.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests = [...modalManifests, ...workspaceManifests, ...conditionManifests];
